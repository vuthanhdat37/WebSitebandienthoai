using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSiteBanHang.Models;
using WebSiteBanHang.Repositories;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace WebSiteBanHang.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ShoppingCart _shoppingCart;
        private readonly IOrderRepository _orderRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(IProductRepository productRepository, ShoppingCart shoppingCart, IOrderRepository orderRepository, UserManager<ApplicationUser> userManager)
        {
            _productRepository = productRepository;
            _shoppingCart = shoppingCart;
            _orderRepository = orderRepository;
            _userManager = userManager;
        }

        // GET: /Order/Checkout
        public IActionResult Checkout()
        {
            var items = _shoppingCart.GetShoppingCartItems();
            _shoppingCart.ShoppingCartItems = items;

            if (_shoppingCart.ShoppingCartItems.Count == 0)
            {
                ModelState.AddModelError("", "Giỏ hàng của bạn đang trống, hãy thêm sản phẩm vào trước khi thanh toán.");
                return RedirectToAction("Index", "ShoppingCart");
            }

            var viewModel = new CheckoutViewModel
            {
                Order = new Order(),
                CartItems = items,
                CartTotal = _shoppingCart.GetShoppingCartTotal()
            };

            return View(viewModel);
        }

        // POST: /Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel viewModel)
        {
            var items = _shoppingCart.GetShoppingCartItems();
            _shoppingCart.ShoppingCartItems = items;

            if (_shoppingCart.ShoppingCartItems.Count == 0)
            {
                ModelState.AddModelError("", "Giỏ hàng của bạn đang trống.");
            }

            if (ModelState.IsValid)
            {
                // Set the initial order status
                viewModel.Order.OrderStatus = "Chờ xác nhận";
                viewModel.Order.PaymentMethod = viewModel.PaymentMethod;
                await _orderRepository.CreateOrderAsync(viewModel.Order);

                // Decrease product quantity in stock
                foreach (var cartItem in items)
                {
                    var product = await _productRepository.GetByIdAsync(cartItem.Product.Id);
                    if (product != null)
                    {
                        product.Quantity -= cartItem.Quantity;
                        // Optional: Add check to ensure quantity doesn't go negative, though validation should prevent this.
                        if(product.Quantity < 0)
                        {
                            product.Quantity = 0;
                        }
                        await _productRepository.UpdateAsync(product);
                    }
                }

                await _shoppingCart.ClearCartAsync();
                // Redirect to a new Order History page
                return RedirectToAction("OrderHistory", "Account");
            }

            // Nếu model không hợp lệ, điền lại thông tin và hiển thị lại view
            viewModel.CartItems = items;
            viewModel.CartTotal = _shoppingCart.GetShoppingCartTotal();
            return View(viewModel);
        }

        // GET: /Order/OrderConfirmation
        public IActionResult OrderConfirmation()
        {
            return View();
        }

        // AJAX: Get user profile information for auto-fill
        [HttpGet]
        public async Task<IActionResult> GetUserProfile()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng." });
                }

                // Check if user has complete profile information
                bool hasCompleteInfo = !string.IsNullOrEmpty(user.FullName) && 
                                      !string.IsNullOrEmpty(user.Address) && 
                                      !string.IsNullOrEmpty(user.PhoneNumber);

                if (!hasCompleteInfo)
                {
                    return Json(new { 
                        success = false, 
                        message = "Thông tin cá nhân chưa đầy đủ. Vui lòng cập nhật thông tin trong trang quản lý tài khoản.",
                        redirectToManage = true 
                    });
                }

                return Json(new { 
                    success = true, 
                    data = new {
                        fullName = user.FullName,
                        phoneNumber = user.PhoneNumber,
                        address = user.Address
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi lấy thông tin người dùng." });
            }
        }
    }
} 