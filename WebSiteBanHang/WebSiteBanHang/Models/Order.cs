using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSiteBanHang.Models
{
    public class Order
    {
        public int Id { get; set; }

        [ValidateNever]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng.")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; }
        
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        // New fields for Order Status and Payment
        public string? OrderStatus { get; set; }
        public string? PaymentMethod { get; set; }
        public string? VnpayTxnRef { get; set; } // Mã giao dịch của đơn hàng
        public string? VnpayTransactionNo { get; set; } // Mã giao dịch của VNPay
        public DateTime? VnpayPayDate { get; set; } // Thời gian thanh toán

        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận.")]
        [Display(Name = "Họ tên người nhận")]
        public string ReceiverName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại người nhận.")]
        [Display(Name = "Số điện thoại người nhận")]
        public string ReceiverPhone { get; set; }

        [ValidateNever]
        public List<OrderDetail> OrderDetails { get; set; }
    }
} 