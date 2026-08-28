using CafePos.Data;
using CafePos.Models;
using CafePos.Services.Implementations;
using CafePos.ViewModels;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CafePos.Tests;

public class OrderServiceTests
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
    public async Task CreateOrderAsync_ValidCart_PersistsOrderAndLineItemsToDatabase()
    {
        // Arrange
        using var db = CreateInMemoryDbContext(nameof(CreateOrderAsync_ValidCart_PersistsOrderAndLineItemsToDatabase));
        var orderService = new OrderService(db);
        var cartItems = new List<CartItem>
        {
            new CartItem(_espresso, 2),  // $5.00
            new CartItem(_croissant, 1)  // $3.25
        };

        // Act
        var order = await orderService.CreateOrderAsync(cartItems, PaymentMethod.Cash);

        // Assert
        Assert.True(order.Id > 0);
        Assert.StartsWith("ORD-", order.OrderNumber);
        Assert.Equal(8.25m, order.Subtotal);
        Assert.Equal(0.70m, order.Tax); // $8.25 * 0.085 = 0.70125 -> $0.70
        Assert.Equal(8.95m, order.GrandTotal);
        Assert.Equal(PaymentMethod.Cash, order.PaymentMethod);
        Assert.Equal(2, order.Items.Count);

        // Verify entity in EF Core InMemory DB
        var dbOrder = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == order.Id);
        Assert.NotNull(dbOrder);
        Assert.Equal(2, dbOrder.Items.Count);
        Assert.Equal("Espresso", dbOrder.Items[0].ProductName);
        Assert.Equal(2.50m, dbOrder.Items[0].UnitPrice);
        Assert.Equal(2, dbOrder.Items[0].Quantity);
        Assert.Equal(5.00m, dbOrder.Items[0].TotalPrice);
    }

    [Fact]
    public async Task CreateOrderAsync_SnapshotsHistoricalUnitPrice_IgnoresFuturePriceChanges()
    {
        // Arrange
        using var db = CreateInMemoryDbContext(nameof(CreateOrderAsync_SnapshotsHistoricalUnitPrice_IgnoresFuturePriceChanges));
        var orderService = new OrderService(db);
        var cartItems = new List<CartItem>
        {
            new CartItem(_espresso, 1) // UnitPrice $2.50
        };

        // Act - Save order
        var order = await orderService.CreateOrderAsync(cartItems, PaymentMethod.Card);

        // Mutate original catalog product price
        _espresso.Price = 4.00m;

        // Assert - Historical snapshot in OrderItem is still $2.50
        var dbOrder = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == order.Id);
        Assert.NotNull(dbOrder);
        Assert.Equal(2.50m, dbOrder.Items[0].UnitPrice);
        Assert.Equal(2.50m, dbOrder.Items[0].TotalPrice);
    }

    [Fact]
    public async Task CreateOrderAsync_EmptyCart_ThrowsInvalidOperationException()
    {
        // Arrange
        using var db = CreateInMemoryDbContext(nameof(CreateOrderAsync_EmptyCart_ThrowsInvalidOperationException));
        var orderService = new OrderService(db);
        var emptyCartItems = new List<CartItem>();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            orderService.CreateOrderAsync(emptyCartItems, PaymentMethod.Cash));
    }

    [Fact]
    public async Task CreateOrderAsync_CardPayment_StoresPaymentMethodCard()
    {
        // Arrange
        using var db = CreateInMemoryDbContext(nameof(CreateOrderAsync_CardPayment_StoresPaymentMethodCard));
        var orderService = new OrderService(db);
        var cartItems = new List<CartItem> { new CartItem(_croissant, 1) };

        // Act
        var order = await orderService.CreateOrderAsync(cartItems, PaymentMethod.Card);

        // Assert
        Assert.Equal(PaymentMethod.Card, order.PaymentMethod);
    }

    [Fact]
    public async Task CheckoutViewModel_ClearsCartOnlyAfterSuccessfulOrderPersistence()
    {
        // Arrange
        using var db = CreateInMemoryDbContext(nameof(CheckoutViewModel_ClearsCartOnlyAfterSuccessfulOrderPersistence));
        var orderService = new OrderService(db);
        var cartService = new CartService();
        cartService.AddItem(_espresso, 2);

        var checkoutViewModel = new CheckoutViewModel(cartService, orderService);
        Assert.Equal(2, cartService.TotalItemCount);

        // Act
        await checkoutViewModel.ProcessCheckoutAsync();

        // Assert - Cart is now cleared only after successful save
        Assert.Equal(0, cartService.TotalItemCount);
        Assert.True(cartService.Items.Count == 0);
        Assert.Single(db.Orders);
    }
}
