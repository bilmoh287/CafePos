using CafePos.Models;

namespace CafePos.Services.Interfaces;

public interface IProductService
{
    Task<List<Product>> GetProductsAsync(string? searchQuery = null, string? category = null);
    Task<List<string>> GetCategoriesAsync();
}
