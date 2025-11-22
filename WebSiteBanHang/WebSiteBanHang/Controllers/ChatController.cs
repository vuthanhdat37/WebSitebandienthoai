using Microsoft.AspNetCore.Mvc;
using WebSiteBanHang.Services;
using WebSiteBanHang.ViewModels;

namespace WebSiteBanHang.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost]
        public async Task<IActionResult> PostMessage([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Message cannot be empty.");
            }

            try
            {
                var response = await _chatService.GetResponseAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log the exception (in a real app)
                return StatusCode(500, "An internal error occurred.");
            }
        }
    }
} 