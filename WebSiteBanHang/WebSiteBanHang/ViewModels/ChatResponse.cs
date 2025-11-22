using WebSiteBanHang.Models;

namespace WebSiteBanHang.ViewModels
{
    public class ChatResponse
    {
        public string? Introduction { get; set; }
        public List<RecommendationPair> Recommendations { get; set; } = new List<RecommendationPair>();
    }
} 