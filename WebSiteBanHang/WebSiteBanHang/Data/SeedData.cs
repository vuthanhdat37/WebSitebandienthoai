using Microsoft.AspNetCore.Identity;
using WebSiteBanHang.Models;
using Microsoft.EntityFrameworkCore;

namespace WebSiteBanHang.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roleNames = { "Admin", "Customer", "Company", "Employee" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminUser = await userManager.FindByEmailAsync("admin@gmail.com");
            if (adminUser == null)
            {
                var newAdminUser = new ApplicationUser
                {
                    UserName = "admin@gmail.com",
                    Email = "admin@gmail.com",
                    FullName = "Quản Trị Viên",
                    EmailConfirmed = true, 
                };
                var result = await userManager.CreateAsync(newAdminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdminUser, "Admin");
                }
            }

            // Seed Categories and Products
            if (!context.Categories.Any())
            {
                var categories = new Category[]
                {
                    new Category { Name = "Apple" },
                    new Category { Name = "Samsung" }
                };
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            if (!context.Products.Any())
            {
                var products = new Product[]
                {
                    new Product
                    {
                        Name = "iPhone 15 Pro Max",
                        Description = "Điện thoại iPhone 15 Pro Max với chip A17 Pro, khung titan và hệ thống camera chuyên nghiệp.",
                        Price = 34990000,
                        Manufacturer = "Apple",
                        Color = "Titan Tự nhiên",
                        StorageGB = 256,
                        RamGB = 8,
                        ScreenSize = "6.7 inch",
                        CameraSpec = "Chính 48MP & Phụ 12MP",
                        ImageUrl = "/images/products/iphone-15-pro-max.jpg",
                        CategoryId = context.Categories.First(c => c.Name == "Apple").Id
                    },
                    new Product
                    {
                        Name = "Samsung Galaxy S24 Ultra",
                        Description = "Trải nghiệm sức mạnh của Galaxy AI trên S24 Ultra, camera 200MP và bút S Pen.",
                        Price = 33990000,
                        Manufacturer = "Samsung",
                        Color = "Xám Titan",
                        StorageGB = 256,
                        RamGB = 12,
                        ScreenSize = "6.8 inch",
                        CameraSpec = "Chính 200MP & Phụ 12MP",
                        ImageUrl = "/images/products/samsung-s24-ultra.jpg",
                        CategoryId = context.Categories.First(c => c.Name == "Samsung").Id
                    },
                    new Product
                    {
                        Name = "iPhone 13",
                        Description = "Hiệu năng ấn tượng với chip A15 Bionic và camera kép tiên tiến.",
                        Price = 17290000,
                        Manufacturer = "Apple",
                        Color = "Xanh lá",
                        StorageGB = 128,
                        RamGB = 4,
                        ScreenSize = "6.1 inch",
                        CameraSpec = "Chính 12MP & Phụ 12MP",
                        ImageUrl = "/images/products/iphone-13.jpg",
                        CategoryId = context.Categories.First(c => c.Name == "Apple").Id
                    }
                };
                context.Products.AddRange(products);
                context.SaveChanges();
            }
        }
    }
} 