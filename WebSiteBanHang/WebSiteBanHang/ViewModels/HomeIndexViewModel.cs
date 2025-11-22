using WebSiteBanHang.Models;
using System.Collections.Generic;

namespace WebSiteBanHang.ViewModels
{
    public class HomeIndexViewModel
    {
        public List<Product>? FeaturedProducts { get; set; }
        public List<Category>? Categories { get; set; }
        // Możemy tu dodać więcej właściwości dla innych sekcji w przyszłości
        // np. List<Testimonial> Testimonials { get; set; }
    }
} 