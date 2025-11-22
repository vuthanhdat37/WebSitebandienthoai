using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebSiteBanHang.Models;
using WebSiteBanHang.Repositories;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using WebSiteBanHang.Areas.Admin.ViewModels;

namespace WebSiteBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Company")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        // Action to display list of products for admin
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }
        
        // GET: Admin/Product/Add
        public async Task<IActionResult> Add()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        // POST: Admin/Product/Add
        [HttpPost]
        public async Task<IActionResult> Add(Product product, IFormFile imageUrl)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl != null && imageUrl.Length > 0)
                {
                    product.ImageUrl = await SaveImage(imageUrl);
                }

                await _productRepository.AddAsync(product);
                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(product);
        }

        // GET: Admin/Product/Update/5
        public async Task<IActionResult> Update(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Admin/Product/Update/5
        [HttpPost]
        public async Task<IActionResult> Update(int id, Product product, IFormFile imageUrl)
        {
            ModelState.Remove("ImageUrl");

            if (id != product.Id) return NotFound();

            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }
            // Create a list to store validation errors
            var validationErrors = new List<string>();

            // Manual validation
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                validationErrors.Add("Tên sản phẩm là bắt buộc.");
            }
            if (product.Price <= 0)
            {
                validationErrors.Add("Giá sản phẩm phải lớn hơn 0.");
            }
            if (product.CategoryId == 0)
            {
                validationErrors.Add("Vui lòng chọn một danh mục.");
            }

            // If there are validation errors, add them to ModelState and return the view
            if (validationErrors.Any())
            {
                foreach (var error in validationErrors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                var categoriesError = await _categoryRepository.GetAllAsync();
                ViewBag.Categories = new SelectList(categoriesError, "Id", "Name", product.CategoryId);
                return View(product);
            }

            // Update the existing product's properties with the new values
            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Description = product.Description;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.Manufacturer = product.Manufacturer;
            existingProduct.Color = product.Color;
            existingProduct.StorageGB = product.StorageGB;
            existingProduct.RamGB = product.RamGB;
            existingProduct.ScreenSize = product.ScreenSize;
            existingProduct.CameraSpec = product.CameraSpec;
            existingProduct.DiscountPrice = product.DiscountPrice;

            if (imageUrl != null && imageUrl.Length > 0)
            {
                existingProduct.ImageUrl = await SaveImage(imageUrl);
            }
                
            await _productRepository.UpdateAsync(existingProduct);
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Product/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: Admin/Product/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            // For AJAX call, return success
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Ok();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Product/DeleteMultiple
        [HttpPost]
        public async Task<IActionResult> DeleteMultiple(int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                return BadRequest("Không có ID nào được cung cấp để xóa.");
            }

            foreach (var id in ids)
            {
                await _productRepository.DeleteAsync(id);
            }

            return Ok(); // Return success for AJAX call
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkUpdateDiscounts(List<ProductDiscountViewModel> productDiscounts)
        {
            if (productDiscounts == null || !productDiscounts.Any())
            {
                TempData["ErrorMessage"] = "Không có dữ liệu giảm giá nào được gửi.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var discountInfo in productDiscounts)
            {
                var product = await _productRepository.GetByIdAsync(discountInfo.ProductId);
                if (product != null)
                {
                    decimal? newDiscountPrice = null;

                    // Priority 1: Direct discount price is set
                    if (discountInfo.DiscountPrice.HasValue && discountInfo.DiscountPrice > 0)
                    {
                        if (discountInfo.DiscountPrice < product.Price)
                        {
                            newDiscountPrice = discountInfo.DiscountPrice;
                        }
                    }
                    // Priority 2: Percentage is set
                    else if (discountInfo.DiscountPercent.HasValue && discountInfo.DiscountPercent > 0 && discountInfo.DiscountPercent < 100)
                    {
                        newDiscountPrice = product.Price * (1 - (discountInfo.DiscountPercent.Value / 100m));
                    }
                    
                    product.DiscountPrice = newDiscountPrice;
                    await _productRepository.UpdateAsync(product);
                }
            }

            TempData["SuccessMessage"] = "Cập nhật giảm giá thành công!";
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/images", image.FileName);
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + image.FileName;
        }
    }
} 