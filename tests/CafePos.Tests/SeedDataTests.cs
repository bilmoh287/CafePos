using CafePos.Data;
using CafePos.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CafePos.Tests;

public class SeedDataTests
{
    private AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task InitializeAsync_SeedsExactSixProductsFromSpecification()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var seeder = new DbInitializer(context);

        // Act
        await seeder.InitializeAsync();
        var products = await context.Products.OrderBy(p => p.SKU).ToListAsync();

        // Assert
        Assert.Equal(6, products.Count);
        Assert.Equal("SKU-001", products[0].SKU);
        Assert.Equal("Espresso", products[0].Name);
        Assert.Equal(2.50m, products[0].Price);
        Assert.Equal("Beverages", products[0].Category);

        Assert.Equal("SKU-002", products[1].SKU);
        Assert.Equal("Croissant", products[1].Name);
        Assert.Equal(3.25m, products[1].Price);
        Assert.Equal("Bakery", products[1].Category);

        Assert.Equal("SKU-006", products[5].SKU);
        Assert.Equal("Turkey Sandwich", products[5].Name);
        Assert.Equal(6.95m, products[5].Price);
        Assert.Equal("Food", products[5].Category);
    }

    [Fact]
    public async Task InitializeAsync_ExecutedMultipleTimes_DoesNotDuplicateProducts()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var seeder = new DbInitializer(context);

        // Act - Run seeding twice
        await seeder.InitializeAsync();
        await seeder.InitializeAsync();

        var count = await context.Products.CountAsync();

        // Assert - Count must remain exactly 6
        Assert.Equal(6, count);
    }
}
