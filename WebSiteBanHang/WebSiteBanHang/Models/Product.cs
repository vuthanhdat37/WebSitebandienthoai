using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSiteBanHang.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Range(0.01, 1000000000.00)]
        [Precision(12, 2)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải là một số không âm.")]
        public int Quantity { get; set; }

        [Display(Name = "Tồn kho tối thiểu")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải là một số không âm.")]
        public int MinQuantity { get; set; } = 10; // Default low-stock threshold

        [Display(Name = "Tồn kho tối đa")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải là một số dương.")]
        public int? MaxQuantity { get; set; } // Optional max stock level

        [Display(Name = "Giá giảm")]
        [Range(0.01, 1000000000.00)]
        [Precision(12, 2)]
        public decimal? DiscountPrice { get; set; }

        [NotMapped]
        public decimal EffectivePrice => DiscountPrice ?? Price;

        [NotMapped]
        public int? DiscountPercent
        {
            get
            {
                if (!DiscountPrice.HasValue || Price == 0)
                {
                    return null;
                }
                return (int)Math.Round((1 - (DiscountPrice.Value / Price)) * 100);
            }
        }

        public string Description { get; set; }

        // Các thuộc tính mới cho điện thoại
        [StringLength(50)]
        public string? Manufacturer { get; set; } // Hãng sản xuất

        [StringLength(50)]
        public string? Color { get; set; } // Màu sắc

        public int? StorageGB { get; set; } // Dung lượng lưu trữ (GB)

        public int? RamGB { get; set; } // Dung lượng RAM (GB)

        [StringLength(50)]
        public string? ScreenSize { get; set; } // Kích thước màn hình

        [StringLength(100)]
        public string? CameraSpec { get; set; } // Thông số camera

        public string? ImageUrl { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public List<ProductImage>? ProductImages { get; set; }
    }
}
