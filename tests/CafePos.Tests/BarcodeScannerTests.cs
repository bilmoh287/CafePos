using CafePos.Models;
using CafePos.Services.Implementations;
using CafePos.Services.Interfaces;
using CafePos.ViewModels;
using Xunit;

namespace CafePos.Tests;

public class BarcodeScannerTests
{
    private class FakeProductService : IProductService
    {
        public Task<List<Product>> GetProductsAsync(string? searchQuery = null, string? category = null)
        {
            var products = new List<Product>
            {
                new Product { Id = 1, SKU = "SKU-001", Name = "Espresso", Price = 2.50m, Category = "Beverages" },
                new Product { Id = 2, SKU = "SKU-002", Name = "Croissant", Price = 3.50m, Category = "Bakery" }
            };

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                products = products.Where(p => string.Equals(p.SKU, searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return Task.FromResult(products);
        }

        public Task<List<string>> GetCategoriesAsync() => Task.FromResult(new List<string> { "Beverages", "Bakery" });
    }

    private class FakeScannerService : IBarcodeScannerService
    {
        public Task<string> ScanBarcodeAsync(string? defaultSku = null) 
            => Task.FromResult(string.IsNullOrWhiteSpace(defaultSku) ? "SKU-001" : defaultSku);
    }

    [Fact]
    public async Task ScanBarcodeAsync_ReturnsSimulatedSKU()
    {
        // Arrange
        var scannerService = new MockBarcodeScannerService();

        // Act
        string scannedSku = await scannerService.ScanBarcodeAsync("SKU-002");

        // Assert
        Assert.Equal("SKU-002", scannedSku);
    }

    [Fact]
    public async Task ProductsViewModel_ScanBarcodeCommand_AddsProductToCartWhenFound()
    {
        // Arrange
        var productService = new FakeProductService();
        var cartService = new CartService();
        var scannerService = new FakeScannerService();

        var viewModel = new ProductsViewModel(productService, cartService, scannerService)
        {
            SimulatedSku = "SKU-001"
        };

        // Act
        await viewModel.ScanBarcodeCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(1, cartService.TotalItemCount);
        Assert.Equal("Espresso", cartService.Items[0].Product.Name);
        Assert.Contains("Espresso", viewModel.StatusMessage);
    }

    [Fact]
    public async Task ProductsViewModel_ScanBarcodeCommand_HandlesUnknownSKUGracefullyWithoutCrashing()
    {
        // Arrange
        var productService = new FakeProductService();
        var cartService = new CartService();
        var scannerService = new FakeScannerService();

        var viewModel = new ProductsViewModel(productService, cartService, scannerService)
        {
            SimulatedSku = "SKU-999"
        };

        // Act
        await viewModel.ScanBarcodeCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(0, cartService.TotalItemCount);
        Assert.Contains("not found", viewModel.StatusMessage);
    }
}
