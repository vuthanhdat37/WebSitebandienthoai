using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSiteBanHang.Models;
using WebSiteBanHang.Repositories;
using System.Threading.Tasks;
using System.Linq;

namespace WebSiteBanHang.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ShoppingCart _shoppingCart;

        public ShoppingCartController(IProductRepository productRepository, ShoppingCart shoppingCart)
        {
            _productRepository = productRepository;
            _shoppingCart = shoppingCart;
        }

        public IActionResult Index()
        {
            var items = _shoppingCart.GetShoppingCartItems();
            _shoppingCart.ShoppingCartItems = items;

            var viewModel = new ShoppingCartViewModel
            {
                ShoppingCart = _shoppingCart,
                ShoppingCartTotal = _shoppingCart.GetShoppingCartTotal()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var selectedProduct = await _productRepository.GetByIdAsync(productId);
            if (selectedProduct != null)
            {
                await _shoppingCart.AddToCartAsync(selectedProduct, quantity);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var selectedProduct = await _productRepository.GetByIdAsync(productId);
            if (selectedProduct == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tìm thấy." });
            }

            await _shoppingCart.ClearFromCartAsync(selectedProduct);

            return Json(new
            {
                success = true,
                reload = true // Signal to reload the page to show empty cart message
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            var selectedProduct = await _productRepository.GetByIdAsync(productId);
            if (selectedProduct == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tìm thấy." });
            }

            if (quantity <= 0)
            {
                await _shoppingCart.ClearFromCartAsync(selectedProduct);
                return Json(new { success = true, reload = true }); // Signal reload if item is removed
            }

            await _shoppingCart.SetCartItemQuantityAsync(selectedProduct, quantity);

            var item = _shoppingCart.GetShoppingCartItems().FirstOrDefault(i => i.Product.Id == productId);

            return Json(new
            {
                success = true,
                itemQuantity = item?.Quantity ?? 0,
                itemTotal = (item?.Quantity ?? 0) * (item?.Product.Price ?? 0),
                cartTotal = _shoppingCart.GetShoppingCartTotal(),
                cartCount = _shoppingCart.GetShoppingCartItems().Sum(c => c.Quantity)
            });
        }
    }
} 