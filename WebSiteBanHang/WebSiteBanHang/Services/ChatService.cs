using OpenAI.Interfaces;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using WebSiteBanHang.Models;
using WebSiteBanHang.Repositories;
using WebSiteBanHang.ViewModels;

namespace WebSiteBanHang.Services
{
    // Helper classes for AI JSON response
    public class AiProductRecommendation
    {
        [JsonPropertyName("productId")]
        public int ProductId { get; set; }
        [JsonPropertyName("explanation")]
        public string Explanation { get; set; }
    }

    public class AiResponseObject
    {
        [JsonPropertyName("recommendations")]
        public List<AiProductRecommendation> Recommendations { get; set; }
    }

    public class ChatService : IChatService
    {
        private readonly IOpenAIService _openAiService;
        private readonly IProductRepository _productRepository;
        private readonly string _modelName;

        public ChatService(IOpenAIService openAiService, IProductRepository productRepository, IConfiguration configuration)
        {
            _openAiService = openAiService;
            _productRepository = productRepository;
            _modelName = configuration["OpenAISettings:Model"] ?? "gpt-4o";
        }

        public async Task<ChatResponse> GetResponseAsync(ChatRequest request)
        {
            var lowerMessage = request.Message.ToLower().Trim();
            string[] greetingKeywords = { "chào", "hello", "hi", "helo", "alo" };
            var words = lowerMessage.Split(new[] { ' ', ',', '.' }, StringSplitOptions.RemoveEmptyEntries);

            if (words.Length <= 3 && greetingKeywords.Any(kw => lowerMessage.Contains(kw)))
            {
                return new ChatResponse 
                { 
                    Introduction = "Chào bạn! Tôi là trợ lý ảo của Tech World. Bạn đang tìm sản phẩm nào ạ?" 
                };
            }
            
            var allProducts = (await _productRepository.GetAllAsync()).ToList();
            var targetPrice = ParsePrice(request.Message);
            var contextProducts = SelectContextProducts(request.Message, allProducts, targetPrice);

            // If no context products are found for the specific budget, widen the search
            if (!contextProducts.Any() && targetPrice > 0)
            {
                 contextProducts = allProducts
                    .Where(p => p.Category != null && !new List<string> { "phụ kiện điện thoại", "pin dự phòng" }.Contains(p.Category.Name.ToLower()))
                    .OrderBy(p => Math.Abs(p.Price - targetPrice))
                    .Take(5)
                    .ToList();
            }

            var prompt = BuildJsonPrompt(request.Message, contextProducts, targetPrice);

            var completionResult = await _openAiService.ChatCompletion.CreateCompletion(
                new ChatCompletionCreateRequest
                {
                    Messages = new List<ChatMessage>
                    {
                        ChatMessage.FromSystem("Bạn là một chuyên gia tư vấn bán hàng của Tech World. Nhiệm vụ của bạn là phải trả lời dưới dạng một chuỗi JSON object hợp lệ. Object này phải chứa một key duy nhất là `recommendations`, giá trị của key này là một mảng các object, mỗi object chứa 2 key: `productId` (số nguyên) và `explanation` (chuỗi ký tự giải thích)."),
                        ChatMessage.FromUser(prompt)
                    },
                    Model = _modelName,
                    MaxTokens = 1024,
                    Temperature = 0.5f,
                    ResponseFormat = new ResponseFormat { Type = "json_object" } // Force JSON mode
                });

            var finalResponse = new ChatResponse();

            if (completionResult.Successful)
            {
                var jsonResponse = completionResult.Choices.First().Message.Content;
                try
                {
                    var aiResponse = JsonSerializer.Deserialize<AiResponseObject>(jsonResponse);
                    if (aiResponse?.Recommendations != null)
                    {
                        foreach (var rec in aiResponse.Recommendations)
                        {
                            var product = allProducts.FirstOrDefault(p => p.Id == rec.ProductId);
                            if (product != null)
                            {
                                finalResponse.Recommendations.Add(new RecommendationPair
                                {
                                    Explanation = rec.Explanation,
                                    Product = new ChatProductViewModel
                                    {
                                        Id = product.Id,
                                        Name = product.Name,
                                        Price = product.Price,
                                        ImageUrl = product.ImageUrl ?? "/images/placeholder.png"
                                    }
                                });
                            }
                        }
                    }
                }
                catch (JsonException)
                {
                    finalResponse.Introduction = "Xin lỗi, tôi chưa hiểu ý của bạn. Bạn có thể nói rõ hơn về sản phẩm bạn đang tìm kiếm không?";
                }
            }
            
            if (!finalResponse.Recommendations.Any() && string.IsNullOrEmpty(finalResponse.Introduction))
            {
                finalResponse.Introduction = "Xin lỗi, tôi đang gặp chút sự cố. Bạn vui lòng thử lại sau nhé.";
            }

            return finalResponse;
        }

        private List<Product> SelectContextProducts(string userMessage, List<Product> allProducts, decimal targetPrice)
        {
            var lowerMessage = userMessage.ToLower();
            IEnumerable<Product> productsToConsider = allProducts;

            // Step 1: Category Filtering
            var allCategoryNames = allProducts
                .Where(p => p.Category?.Name != null)
                .Select(p => p.Category.Name.ToLower())
                .Distinct()
                .ToList();
            var mentionedCategory = allCategoryNames.FirstOrDefault(c => lowerMessage.Contains(c));

            if (mentionedCategory != null)
            {
                productsToConsider = productsToConsider.Where(p => p.Category?.Name.ToLower() == mentionedCategory);
            }
            else if (lowerMessage.Contains("điện thoại"))
            {
                var accessoryCategories = new List<string> { "phụ kiện điện thoại", "pin dự phòng" };
                productsToConsider = productsToConsider.Where(p => p.Category != null && !accessoryCategories.Contains(p.Category.Name.ToLower()));
            }

            // Step 2: Sorting and Filtering based on Price
            if (targetPrice > 0)
            {
                // User specified a budget. Find products around that budget.
                productsToConsider = productsToConsider
                    .OrderBy(p => Math.Abs(p.Price - targetPrice));
            }
            else
            {
                // User did not specify a budget, check for "cheapest" or "most expensive".
                bool cheapest = lowerMessage.Contains("rẻ nhất") || lowerMessage.Contains("rẻ") || lowerMessage.Contains("thấp nhất");
                bool mostExpensive = lowerMessage.Contains("đắt nhất") || lowerMessage.Contains("cao nhất");

                if (cheapest)
                {
                    productsToConsider = productsToConsider.OrderBy(p => p.Price);
                }
                else if (mostExpensive)
                {
                    productsToConsider = productsToConsider.OrderByDescending(p => p.Price);
                }
                else
                {
                    // Default sort if no price preference is given.
                    productsToConsider = productsToConsider.OrderByDescending(p => p.Id);
                }
            }

            return productsToConsider.Take(5).ToList();
        }

        private decimal ParsePrice(string text)
        {
            var match = Regex.Match(text, @"(\d{1,3})\s*(triệu|tr)");
            if (match.Success)
            {
                return decimal.Parse(match.Groups[1].Value) * 1_000_000;
            }
            return 0;
        }

        private string BuildJsonPrompt(string userMessage, IEnumerable<Product> products, decimal targetPrice)
        {
            var productsJson = JsonSerializer.Serialize(products.Select(p => new { p.Id, p.Name, p.Price, p.Description, p.CameraSpec }));

            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine("Phân tích bối cảnh và yêu cầu sau, sau đó trả về một JSON object hợp lệ theo vai trò đã được chỉ định.");
            promptBuilder.AppendLine("---CONTEXT---");
            promptBuilder.AppendLine("Sản phẩm có sẵn (JSON):");
            promptBuilder.AppendLine(productsJson);
            promptBuilder.AppendLine("---YÊU CẦU---");
            if (targetPrice > 0)
            {
                promptBuilder.AppendLine($"**Ngân sách của khách hàng:** khoảng {targetPrice:N0} VND.");
            }
            promptBuilder.AppendLine($"**Nội dung chat của khách:** \"{userMessage}\"");
            promptBuilder.AppendLine("---NHIỆM VỤ---");
            promptBuilder.AppendLine("Tạo một JSON object chứa key `recommendations`. Với mỗi sản phẩm, giải thích ngắn gọn tại sao nó phù hợp. **Quan trọng: Khi so sánh giá sản phẩm với ngân sách của khách, hãy so sánh cho chính xác.** Ví dụ: nếu ngân sách là 10 triệu và sản phẩm giá 7 triệu, hãy nói là nó 'nằm trong ngân sách' hoặc 'rẻ hơn ngân sách dự kiến'. Luôn cố gắng đề xuất ít nhất một sản phẩm.");

            return promptBuilder.ToString();
        }
    }
} 