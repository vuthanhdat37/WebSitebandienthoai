using WebSiteBanHang.ViewModels;

namespace WebSiteBanHang.Services
{
    public interface IChatService
    {
        Task<ChatResponse> GetResponseAsync(ChatRequest request);
    }
} 