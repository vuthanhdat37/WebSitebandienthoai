using Microsoft.AspNetCore.Mvc;
using WebSiteBanHang.Models;
using System.Threading.Tasks;
using WebSiteBanHang.Repositories;
using WebSiteBanHang.ViewModels;
using System.Linq;

namespace WebSiteBanHang.Controllers
{
    public class SearchController : Controller
    {
        private readonly IProductRepository _productRepository;
        private const int PageSize = 8; // Số sản phẩm trên mỗi trang

        public SearchController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IActionResult> Index(string query, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                // Nếu query rỗng, có thể chuyển hướng về trang chủ hoặc hiển thị trang tìm kiếm trống
                return View(new SearchViewModel
                {
                    Query = query,
                    Results = new List<Product>(),
                    PageNumber = 1,
                    TotalPages = 0
                });
            }

            var allResults = await _productRepository.SearchAsync(query);
            
            var totalItems = allResults.Count();
            var totalPages = (int)System.Math.Ceiling(totalItems / (double)PageSize);

            var pagedResults = allResults
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var viewModel = new SearchViewModel
            {
                Query = query,
                Results = pagedResults,
                PageNumber = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }
    }
} 