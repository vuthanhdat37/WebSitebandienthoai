using Microsoft.EntityFrameworkCore;
using WebSiteBanHang.Data;

namespace WebSiteBanHang.Models
{
    public class ShoppingCart
    {
        private readonly ApplicationDbContext _context;
        public string ShoppingCartId { get; set; }
        public List<CartItem> ShoppingCartItems { get; set; }

        private ShoppingCart(ApplicationDbContext context)
        {
            _context = context;
        }

        public static ShoppingCart GetCart(IServiceProvider services)
        {
            ISession session = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext.Session;
            var context = services.GetService<ApplicationDbContext>();
            string cartId = session.GetString("CartId") ?? Guid.NewGuid().ToString();
            session.SetString("CartId", cartId);

            return new ShoppingCart(context) { ShoppingCartId = cartId };
        }

        public async Task AddToCartAsync(Product product, int quantity)
        {
            var cartItem = await _context.CartItems.SingleOrDefaultAsync(
                s => s.Product.Id == product.Id && s.ShoppingCartId == ShoppingCartId);

            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    ShoppingCartId = ShoppingCartId,
                    Product = product,
                    Quantity = quantity
                };
                _context.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += quantity;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<int> RemoveFromCartAsync(Product product)
        {
            var cartItem = await _context.CartItems.SingleOrDefaultAsync(
                s => s.Product.Id == product.Id && s.ShoppingCartId == ShoppingCartId);

            var localQuantity = 0;
            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                {
                    cartItem.Quantity--;
                    localQuantity = cartItem.Quantity;
                }
                else
                {
                    _context.CartItems.Remove(cartItem);
                }
            }
            await _context.SaveChangesAsync();
            return localQuantity;
        }

        public async Task ClearFromCartAsync(Product product)
        {
            var cartItem = await _context.CartItems.SingleOrDefaultAsync(
                s => s.Product.Id == product.Id && s.ShoppingCartId == ShoppingCartId);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SetCartItemQuantityAsync(Product product, int quantity)
        {
            var cartItem = await _context.CartItems.SingleOrDefaultAsync(
                s => s.Product.Id == product.Id && s.ShoppingCartId == ShoppingCartId);

            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                await _context.SaveChangesAsync();
            }
        }

        public List<CartItem> GetShoppingCartItems()
        {
            return ShoppingCartItems ??= _context.CartItems
                .Where(c => c.ShoppingCartId == ShoppingCartId)
                .Include(s => s.Product)
                .ToList();
        }

        public async Task ClearCartAsync()
        {
            var cartItems = _context.CartItems.Where(cart => cart.ShoppingCartId == ShoppingCartId);
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }

        public decimal GetShoppingCartTotal()
        {
            var total = _context.CartItems
                .Where(c => c.ShoppingCartId == ShoppingCartId)
                .Select(c => (c.Product.DiscountPrice ?? c.Product.Price) * c.Quantity)
                .Sum();
            return total;
        }
    }
} 