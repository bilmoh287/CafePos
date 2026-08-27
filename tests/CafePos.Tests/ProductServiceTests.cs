using CafePos.Data;
using CafePos.Models;
using CafePos.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CafePos.Tests;

public class ProductServiceTests
{
    private AppDbContext CreateDbContextWithSeedData()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        context.Products.AddRange(new List<Product>
        {
            new Product { SKU = "SKU-001", Name = "Espresso", Price = 2.50m, Category = "Beverages" },
            new Product { SKU = "SKU-002", Name = "Croissant", Price = 3.25m, Category = "Bakery" },
            new Product { SKU = "SKU-003", Name = "Iced Tea", Price = 3.00m, Category = "Beverages" },
            new Product { SKU = "SKU-004", Name = "Blueberry Muffin", Price = 3.75m, Category = "Bakery" }
        });
        context.SaveChanges();

        return context;
    }

    [Fact]
    public async Task GetProductsAsync_FilterByCategory_ReturnsOnlyCategoryItems()
    {
        // Arrange
        using var context = CreateDbContextWithSeedData();
        var service = new ProductService(context);

        // Act
        var bakeryItems = await service.GetProductsAsync(category: "Bakery");

        // Assert
        Assert.Equal(2, bakeryItems.Count);
        Assert.All(bakeryItems, p => Assert.Equal("Bakery", p.Category));
    }

    [Fact]
    public async Task GetProductsAsync_SearchQuery_ReturnsMatchingNameOrSKU()
    {
        // Arrange
        using var context = CreateDbContextWithSeedData();
        var service = new ProductService(context);

        // Act
        var searchResult = await service.GetProductsAsync(searchQuery: "tea");

        // Assert
        Assert.Single(searchResult);
        Assert.Equal("Iced Tea", searchResult[0].Name);
    }
}
