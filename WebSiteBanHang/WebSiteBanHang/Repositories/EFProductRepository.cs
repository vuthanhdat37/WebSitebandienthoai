using Microsoft.EntityFrameworkCore;
using WebSiteBanHang.Data;
using WebSiteBanHang.Models;

namespace WebSiteBanHang.Repositories
{
    public class EFProductRepository(ApplicationDbContext context) : IProductRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.Include(p => p.Category).ToListAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                                 .Include(p => p.Category)
                                 .Include(p => p.ProductImages)
                                 .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
        {
            return await _context.Products
                                 .Where(p => p.CategoryId == categoryId)
                                 .Include(p => p.Category)
                                 .ToListAsync();
        }

        public async Task AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product updatedProduct)
        {
            _context.Products.Update(updatedProduct);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Product>> SearchAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Enumerable.Empty<Product>();
            }

            var lowerCaseKeyword = keyword.Trim().ToLower();

            return await _context.Products
                .Include(p => p.Category)
                .Where(p =>
                    p.Name.ToLower().Contains(lowerCaseKeyword) ||
                    (p.Description != null && p.Description.ToLower().Contains(lowerCaseKeyword)) ||
                    (p.Category != null && p.Category.Name.ToLower().Contains(lowerCaseKeyword))
                )
                .ToListAsync();
        }
    }
}
