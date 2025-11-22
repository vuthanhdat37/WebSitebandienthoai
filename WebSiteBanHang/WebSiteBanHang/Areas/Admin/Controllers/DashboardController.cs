using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSiteBanHang.Repositories;
using System.Threading.Tasks;

namespace WebSiteBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IOrderRepository _orderRepository;

        public DashboardController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IActionResult> Index(int days = 7)
        {
            ViewBag.SelectedDays = days;
            var statistics = await _orderRepository.GetDashboardStatisticsAsync(days);
            return View(statistics);
        }
    }
} 