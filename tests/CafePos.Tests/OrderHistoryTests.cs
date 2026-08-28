using CafePos.Data;
using CafePos.Models;
using CafePos.Services.Implementations;
using CafePos.ViewModels;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CafePos.Tests;

public class OrderHistoryTests
{
    private AppDbContext CreateInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new AppDbContext(options);
    }

    private readonly Product _espresso = new Product { Id = 1, SKU = "SKU-001", Name = "Espresso", Price = 2.50m, Category = "Beverages" };
    private readonly Product _croissant = new Product { Id = 2, SKU = "SKU-002", Name = "Croissant", Price = 3.25m, Category = "Bakery" };

    [Fact]
    public async Task LoadOrdersAsync_PopulatesOrdersList_InDescendingOrder()
    {
        // Arrange
        using var db = CreateInMemoryDbContext(nameof(LoadOrdersAsync_PopulatesOrdersList_InDescendingOrder));
        var orderService = new OrderService(db);

        // Seed 2 orders
        var order1 = await orderService.CreateOrderAsync(new List<CartItem> { new CartItem(_espresso, 1) }, PaymentMethod.Cash);
        await Task.Delay(10);
        var order2 = await orderService.CreateOrderAsync(new List<CartItem> { new CartItem(_croissant, 2) }, PaymentMethod.Card);

        var viewModel = new OrderHistoryViewModel(orderService);

        // Act
        await viewModel.LoadOrdersAsync();

        // Assert
        Assert.Equal(2, viewModel.Orders.Count);
        Assert.Equal(order2.OrderNumber, viewModel.Orders[0].OrderNumber); // Most recent order first
        Assert.Equal(order1.OrderNumber, viewModel.Orders[1].OrderNumber);
        Assert.False(viewModel.IsEmpty);
    }

    [Fact]
    public async Task LoadOrdersAsync_NoOrders_SetsIsEmptyTrue()
    {
        // Arrange
        using var db = CreateInMemoryDbContext(nameof(LoadOrdersAsync_NoOrders_SetsIsEmptyTrue));
        var orderService = new OrderService(db);
        var viewModel = new OrderHistoryViewModel(orderService);

        // Act
        await viewModel.LoadOrdersAsync();

        // Assert
        Assert.Empty(viewModel.Orders);
        Assert.True(viewModel.IsEmpty);
    }

    [Fact]
    public void SelectOrder_SetsSelectedOrderAndIsShowingReceiptTrue()
    {
        // Arrange
        using var db = CreateInMemoryDbContext(nameof(SelectOrder_SetsSelectedOrderAndIsShowingReceiptTrue));
        var orderService = new OrderService(db);
        var viewModel = new OrderHistoryViewModel(orderService);

        var sampleOrder = new Order { Id = 10, OrderNumber = "ORD-TEST-001", GrandTotal = 15.00m };

        // Act
        viewModel.SelectOrder(sampleOrder);

        // Assert
        Assert.NotNull(viewModel.SelectedOrder);
        Assert.Equal("ORD-TEST-001", viewModel.SelectedOrder.OrderNumber);
        Assert.True(viewModel.IsShowingReceipt);

        // Act - Close Receipt
        viewModel.CloseReceipt();

        // Assert
        Assert.Null(viewModel.SelectedOrder);
        Assert.False(viewModel.IsShowingReceipt);
    }
}
