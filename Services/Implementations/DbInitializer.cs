using CafePos.Data;
using CafePos.Models;
using CafePos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CafePos.Services.Implementations;

public class DbInitializer : IDbInitializer
{
    private readonly AppDbContext _context;

    public DbInitializer(AppDbContext context)
    {
        _context = context;
    }

    public async Task InitializeAsync()
    {
        // Ensure SQLite database and schema exist
        await _context.Database.EnsureCreatedAsync();

        // Idempotent seeding check: only seed if product catalog is empty
        if (!await _context.Products.AnyAsync())
        {
            var seedProducts = new List<Product>
            {
                new Product { SKU = "SKU-001", Name = "Espresso", Price = 2.50m, Category = "Beverages" },
                new Product { SKU = "SKU-002", Name = "Croissant", Price = 3.25m, Category = "Bakery" },
                new Product { SKU = "SKU-003", Name = "Iced Tea", Price = 3.00m, Category = "Beverages" },
                new Product { SKU = "SKU-004", Name = "Blueberry Muffin", Price = 3.75m, Category = "Bakery" },
                new Product { SKU = "SKU-005", Name = "Bottled Water", Price = 1.50m, Category = "Beverages" },
                new Product { SKU = "SKU-006", Name = "Turkey Sandwich", Price = 6.95m, Category = "Food" }
            };

            await _context.Products.AddRangeAsync(seedProducts);
            await _context.SaveChangesAsync();
        }
    }
}
