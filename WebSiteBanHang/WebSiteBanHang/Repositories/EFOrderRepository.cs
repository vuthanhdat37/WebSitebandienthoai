using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebSiteBanHang.Data;
using WebSiteBanHang.Models;
using WebSiteBanHang.Areas.Admin.ViewModels;

namespace WebSiteBanHang.Repositories
{
    public class EFOrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ShoppingCart _shoppingCart;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EFOrderRepository(ApplicationDbContext context, 
                               ShoppingCart shoppingCart, 
                               UserManager<ApplicationUser> userManager, 
                               IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _shoppingCart = shoppingCart;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task CreateOrderAsync(Order order)
        {
            // Lấy thông tin người dùng đang đăng nhập
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            order.UserId = userId;
            order.OrderDate = DateTime.Now;
            order.TotalPrice = _shoppingCart.GetShoppingCartTotal();

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // Lưu để lấy được OrderId

            var shoppingCartItems = _shoppingCart.GetShoppingCartItems();

            foreach (var item in shoppingCartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.Id, // Gán Id của Order vừa tạo
                    ProductId = item.Product.Id,
                    Quantity = item.Quantity,
                    Price = item.Product.EffectivePrice
                };
                _context.OrderDetails.Add(orderDetail);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.ApplicationUser) // Include user details
                .Include(o => o.OrderDetails)   // Eagerly load order details
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.OrderStatus = status;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task UpdateOrderAsync(Order order)
        {
            // Recalculate TotalPrice before saving
            order.TotalPrice = order.OrderDetails.Sum(d => d.Quantity * d.Price);

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task RejectOrderAsync(int orderId)
        {
            await UpdateOrderStatusAsync(orderId, "Đã từ chối");
        }

        public async Task<DashboardViewModel> GetDashboardStatisticsAsync(int days = 30)
        {
            var successfulStatus = "Đã thanh toán";
            var confirmedStatus = "Đã xác nhận";
            var startDate = DateTime.Now.Date.AddDays(-days + 1);

            var successfulOrders = await _context.Orders
                .Where(o => o.OrderStatus == successfulStatus && o.OrderDate >= startDate)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .ToListAsync();

            var totalConfirmedOrders = await _context.Orders
                .Where(o => o.OrderStatus == confirmedStatus && o.OrderDate >= startDate)
                .CountAsync();

            if (!successfulOrders.Any() && totalConfirmedOrders == 0)
            {
                return new DashboardViewModel(); // Return empty view model if no data
            }

            var totalRevenue = successfulOrders.Sum(o => o.TotalPrice);
            var totalOrders = successfulOrders.Count;

            var allOrderDetails = successfulOrders.SelectMany(o => o.OrderDetails).ToList();
            var totalProductsSold = allOrderDetails.Sum(od => od.Quantity);

            var topSellingProducts = allOrderDetails
                .GroupBy(od => od.Product)
                .Select(g => new TopProductViewModel
                {
                    ProductName = g.Key.Name,
                    UnitsSold = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(p => p.UnitsSold)
                .Take(5)
                .ToList();
            
            var mostPopularProduct = topSellingProducts.FirstOrDefault()?.ProductName ?? "N/A";

            var revenueByDate = successfulOrders
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new RevenueDataPoint
                {
                    Period = g.Key.ToString("dd/MM"),
                    Revenue = g.Sum(o => o.TotalPrice)
                })
                .OrderBy(r => r.Period)
                .ToList();

            var recentOrders = successfulOrders
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .ToList();

            return new DashboardViewModel
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TotalProductsSold = totalProductsSold,
                TotalConfirmedOrders = totalConfirmedOrders,
                MostPopularProduct = mostPopularProduct,
                RevenueByDate = revenueByDate,
                TopSellingProducts = topSellingProducts,
                RecentSuccessfulOrders = recentOrders
            };
        }
    }
} 