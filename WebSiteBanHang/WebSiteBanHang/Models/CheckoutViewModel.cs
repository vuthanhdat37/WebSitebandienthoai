using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WebSiteBanHang.Models
{
    public class CheckoutViewModel
    {
        public Order Order { get; set; }
        
        [ValidateNever]
        public List<CartItem> CartItems { get; set; }
        public decimal CartTotal { get; set; }
        public string PaymentMethod { get; set; }
    }
} 