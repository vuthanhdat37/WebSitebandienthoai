using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebSiteBanHang.Models;
using WebSiteBanHang.Repositories;
using WebSiteBanHang.Services;

namespace WebSiteBanHang.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IVnpayService _vnpayService;
        private readonly IOrderRepository _orderRepository;

        public PaymentController(IVnpayService vnpayService, IOrderRepository orderRepository)
        {
            _vnpayService = vnpayService;
            _orderRepository = orderRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Order order, string PaymentMethod)
        {
            // ... existing code ...
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> CreateVnpayPayment(int orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }

            var paymentModel = new VnPaymentRequestModel
            {
                Amount = (double)order.TotalPrice,
                CreatedDate = order.OrderDate,
                Description = $"Thanh toán đơn hàng #{order.Id}",
                FullName = order.ApplicationUser.FullName,
                OrderId = order.Id
            };
            
            var paymentUrl = _vnpayService.CreatePaymentUrl(HttpContext, paymentModel);
            return Redirect(paymentUrl);
        }

        [AllowAnonymous]
        public async Task<IActionResult> VnpayReturn()
        {
            var response = _vnpayService.PaymentExecute(Request.Query);

            if (response == null || !response.Success)
            {
                TempData["PaymentMessage"] = "Thanh toán VNPay thất bại.";
                return RedirectToAction("OrderFailure");
            }

            if (long.TryParse(response.OrderId, out long orderIdAsLong))
            {
                var order = await _orderRepository.GetOrderByIdAsync((int)orderIdAsLong);
                if (order != null && order.VnpayPayDate == null) // Chỉ cập nhật nếu chưa được xử lý
                {
                    order.OrderStatus = "Đã thanh toán";
                    order.VnpayTransactionNo = response.VnPayTransactionNo;
                    order.VnpayPayDate = DateTime.Now;
                    await _orderRepository.UpdateOrderAsync(order);
                }
            }

            TempData["PaymentMessage"] = "Thanh toán VNPay thành công!";
            return RedirectToAction("OrderSuccess", new { id = response.OrderId });
        }

        [AllowAnonymous]
        public async Task<IActionResult> VnpayIpn()
        {
            var response = _vnpayService.PaymentExecute(Request.Query);

            if (response != null && response.Success && response.VnPayResponseCode == "00")
            {
                if (long.TryParse(response.OrderId, out long orderIdAsLong))
                {
                    var order = await _orderRepository.GetOrderByIdAsync((int)orderIdAsLong);
                    // Chỉ cập nhật trạng thái nếu đơn hàng tồn tại và chưa được thanh toán trước đó
                    if (order != null && order.VnpayPayDate == null)
                    {
                        order.OrderStatus = "Đã thanh toán";
                        order.VnpayTransactionNo = response.VnPayTransactionNo;
                        order.VnpayPayDate = DateTime.Now;
                        await _orderRepository.UpdateOrderAsync(order);

                        // Trả về kết quả thành công cho VNPAY
                        return Ok(new { RspCode = "00", Message = "Confirm Success" });
                    }
                }
            }

            // Trả về lỗi nếu có vấn đề
            return Ok(new { RspCode = "01", Message = "Order not found or already confirmed" });
        }

        public IActionResult OrderSuccess(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }

        public IActionResult OrderFailure()
        {
            return View();
        }
    }
} 