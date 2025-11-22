using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebSiteBanHang.Models;
using WebSiteBanHang.Repositories;
using WebSiteBanHang.ViewModels;
using System.Threading.Tasks;
using System.Linq;

namespace WebSiteBanHang.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index(int? categoryId, string sortOrder)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["PriceSortParm"] = string.IsNullOrEmpty(sortOrder) ? "price_desc" : "";
            ViewData["PriceSortParmAsc"] = sortOrder == "price_asc" ? "price_desc" : "price_asc";

            var categories = await _categoryRepository.GetAllAsync();
            IEnumerable<Product> products;
            string? selectedCategoryName = null;

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                products = await _productRepository.GetByCategoryIdAsync(categoryId.Value);
                var selectedCategory = categories.FirstOrDefault(c => c.Id == categoryId.Value);
                if (selectedCategory != null)
                {
                    selectedCategoryName = selectedCategory.Name;
                }
            }
            else
            {
                products = await _productRepository.GetAllAsync(); 
            }

            switch (sortOrder)
            {
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                default:
                    // Mặc định có thể sắp xếp theo tên hoặc giữ nguyên
                    products = products.OrderBy(p => p.Name);
                    break;
            }

            var viewModel = new ProductIndexViewModel 
            {
                Products = products,
                Categories = categories,
                SelectedCategoryId = categoryId,
                SelectedCategoryName = selectedCategoryName ?? "Tất cả sản phẩm"
            };

            return View(viewModel); 
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View("Details", product);
        }
    }
}
