using WebSiteBanHang.Models;
using System.Collections.Generic;

namespace WebSiteBanHang.ViewModels
{
    public class ProductIndexViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public int? SelectedCategoryId { get; set; }
        public string? SelectedCategoryName { get; set; }
    }
} 