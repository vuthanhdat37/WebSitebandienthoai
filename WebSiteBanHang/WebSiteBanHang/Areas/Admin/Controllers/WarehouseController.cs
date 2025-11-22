using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSiteBanHang.Repositories;
using System.Threading.Tasks;
using WebSiteBanHang.Models;
using System.Collections.Generic;
using System.Linq;

namespace WebSiteBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class WarehouseController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public WarehouseController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        // GET: Admin/Warehouse
        public async Task<IActionResult> Index(string searchString, int? categoryId, string stockStatus)
        {
            var products = await _productRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.ToLower().Contains(searchString.ToLower()));
            }

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(stockStatus))
            {
                switch (stockStatus)
                {
                    case "in-stock":
                        products = products.Where(p => p.Quantity > p.MinQuantity);
                        break;
                    case "low-stock":
                        products = products.Where(p => p.Quantity > 0 && p.Quantity <= p.MinQuantity);
                        break;
                    case "out-of-stock":
                        products = products.Where(p => p.Quantity == 0);
                        break;
                }
            }
            
            ViewData["Categories"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name", categoryId);
            ViewData["CurrentSearch"] = searchString;
            ViewData["CurrentStockStatus"] = stockStatus;

            return View(products.ToList());
        }

        // POST: Admin/Warehouse/BulkUpdate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkUpdate(List<int> productIds, List<int> quantities)
        {
            if (productIds == null || quantities == null || productIds.Count != quantities.Count)
            {
                TempData["ErrorMessage"] = "Dữ liệu cập nhật không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            for (int i = 0; i < productIds.Count; i++)
            {
                var product = await _productRepository.GetByIdAsync(productIds[i]);
                if (product != null)
                {
                    if (quantities[i] >= 0)
                    {
                        product.Quantity = quantities[i];
                        await _productRepository.UpdateAsync(product);
                    }
                }
            }

            TempData["SuccessMessage"] = "Cập nhật tồn kho hàng loạt thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
} 