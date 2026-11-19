using Microsoft.EntityFrameworkCore;
using ShopSphere.Domain.Entities;
using ShopSphere.Domain.Enums;

namespace ShopSphere.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!await context.Users.AnyAsync())
        {
            var users = new List<User>
            {
                new()
                {
                    FullName = "ShopSphere Admin",
                    Email = "admin@shopsphere.local",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = UserRole.Admin
                },
                new()
                {
                    FullName = "Demo Customer",
                    Email = "customer@shopsphere.local",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
                    Role = UserRole.Customer
                }
            };

            await context.Users.AddRangeAsync(users);
        }

        if (!await context.Products.AnyAsync())
        {
            var products = new List<Product>
            {
                new()
                {
                    Name = "Mechanical Wireless Keyboard",
                    Description = "Hot-swappable RGB mechanical keyboard with tactile switches.",
                    Category = "Electronics",
                    Sku = "ELEC-KB-001",
                    Price = 89.99m,
                    StockQuantity = 25,
                    ImageUrl = "https://unsplash.com"
                },
                new()
                {
                    Name = "Ergonomic 4K USB-C Monitor",
                    Description = "27-inch IPS UHD monitor with 99% sRGB and USB-C power delivery.",
                    Category = "Electronics",
                    Sku = "ELEC-MN-002",
                    Price = 349.00m,
                    StockQuantity = 10,
                    ImageUrl = "https://unsplash.com"
                },
                new()
                {
                    Name = "Noise-Cancelling Studio Headphones",
                    Description = "Over-ear wireless ANC headphones with 40-hour battery life.",
                    Category = "Audio",
                    Sku = "AUD-HP-003",
                    Price = 199.50m,
                    StockQuantity = 18,
                    ImageUrl = "https://unsplash.com"
                },
                new()
                {
                    Name = "Minimalist Leather Laptop Backpack",
                    Description = "Water-resistant 16-inch laptop backpack with padded compartments.",
                    Category = "Accessories",
                    Sku = "ACC-BP-004",
                    Price = 74.00m,
                    StockQuantity = 30,
                    ImageUrl = "https://unsplash.com"
                },
                new()
                {
                    Name = "Insulated Stainless Steel Tumbler",
                    Description = "32oz double-wall vacuum insulated bottle keeps drinks cold for 24h.",
                    Category = "Lifestyle",
                    Sku = "LIFE-TM-005",
                    Price = 29.99m,
                    StockQuantity = 50,
                    ImageUrl = "https://unsplash.com"
                },
                new()
                {
                    Name = "Smart Desk LED Light Bar",
                    Description = "Glare-free monitor light bar with adjustable color temperature.",
                    Category = "Accessories",
                    Sku = "ACC-LB-006",
                    Price = 45.00m,
                    StockQuantity = 3, // Low stock to easily test concurrency & stock limits!
                    ImageUrl = "https://unsplash.com"
                }
            };

            await context.Products.AddRangeAsync(products);
        }

        await context.SaveChangesAsync();
    }
}
