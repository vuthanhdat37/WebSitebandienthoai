using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebSiteBanHang.Models;
using WebSiteBanHang.Repositories;

namespace WebSiteBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employee")]
    public class OrderManagementController : Controller
    {
        private readonly IOrderRepository _orderRepository;

        public OrderManagementController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        // GET: /Admin/OrderManagement
        public async Task<IActionResult> Index()
        {
            var orders = await _orderRepository.GetAllOrdersAsync();
            return View(orders);
        }

        // GET: /Admin/OrderManagement/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST: /Admin/OrderManagement/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Order orderViewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ.";
                return View("Details", orderViewModel);
            }

            // Load the existing order from the database
            var existingOrder = await _orderRepository.GetOrderByIdAsync(orderViewModel.Id);
            if (existingOrder == null)
            {
                return NotFound();
            }

            // Update only the allowed properties
            existingOrder.ReceiverName = orderViewModel.ReceiverName;
            existingOrder.ReceiverPhone = orderViewModel.ReceiverPhone;
            existingOrder.ShippingAddress = orderViewModel.ShippingAddress;
            existingOrder.Notes = orderViewModel.Notes;
            existingOrder.OrderStatus = orderViewModel.OrderStatus;

            // Update quantities in OrderDetails
            foreach (var detailViewModel in orderViewModel.OrderDetails)
            {
                var existingDetail = existingOrder.OrderDetails.FirstOrDefault(d => d.Id == detailViewModel.Id);
                if (existingDetail != null)
                {
                    existingDetail.Quantity = detailViewModel.Quantity;
                }
            }

            await _orderRepository.UpdateOrderAsync(existingOrder);
            TempData["Message"] = $"Đơn hàng #{orderViewModel.Id} đã được cập nhật thành công!";
            return RedirectToAction(nameof(Details), new { id = orderViewModel.Id });
        }

        // POST: /Admin/OrderManagement/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            await _orderRepository.UpdateOrderStatusAsync(id, "Đã xác nhận");
            TempData["Message"] = $"Đã duyệt thành công đơn hàng #{id}.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/OrderManagement/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            await _orderRepository.RejectOrderAsync(id);
            TempData["Message"] = $"Đã từ chối đơn hàng #{id}.";
            return RedirectToAction(nameof(Index));
        }
    }
} 