using System.Collections.Generic;
using WebSiteBanHang.Models;

namespace WebSiteBanHang.ViewModels
{
    public class SearchViewModel
    {
        public string Query { get; set; }
        public IEnumerable<Product> Results { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
} 