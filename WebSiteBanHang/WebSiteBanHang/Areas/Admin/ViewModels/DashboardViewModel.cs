namespace WebSiteBanHang.Areas.Admin.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProductsSold { get; set; }
        public int TotalConfirmedOrders { get; set; }
        public string MostPopularProduct { get; set; }
        public List<RevenueDataPoint> RevenueByDate { get; set; }
        public List<TopProductViewModel> TopSellingProducts { get; set; }
        public List<WebSiteBanHang.Models.Order> RecentSuccessfulOrders { get; set; }

        public DashboardViewModel()
        {
            RevenueByDate = new List<RevenueDataPoint>();
            TopSellingProducts = new List<TopProductViewModel>();
            RecentSuccessfulOrders = new List<WebSiteBanHang.Models.Order>();
        }
    }
} 