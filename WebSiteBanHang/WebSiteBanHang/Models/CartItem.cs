using System.ComponentModel.DataAnnotations;

namespace WebSiteBanHang.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } // Navigation property
        public int Quantity { get; set; }
        
        // ID của giỏ hàng mà item này thuộc về
        public string ShoppingCartId { get; set; }
    }
} 