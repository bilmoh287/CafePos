using CafePos.Data;
using CafePos.Models;
using CafePos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CafePos.Services.Implementations;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetProductsAsync(string? searchQuery = null, string? category = null)
    {
        var query = _context.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(p => p.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            string cleanQuery = searchQuery.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(cleanQuery) || p.SKU.ToLower().Contains(cleanQuery));
        }

        return await query.OrderBy(p => p.Category).ThenBy(p => p.Name).ToListAsync();
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        var categories = await _context.Products
            .AsNoTracking()
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        categories.Insert(0, "All");
        return categories;
    }
}
