using CafePos.Data;
using CafePos.Helpers;
using CafePos.Models;
using CafePos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CafePos.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly AppDbContext _dbContext;

    public OrderService(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Order> CreateOrderAsync(IReadOnlyList<CartItem> cartItems, PaymentMethod paymentMethod)
    {
        if (cartItems == null || cartItems.Count == 0)
        {
            throw new InvalidOperationException("Cannot create an order with an empty cart.");
        }

        var subtotal = cartItems.Sum(i => i.TotalPrice);
        var tax = TaxCalculator.CalculateTax(subtotal);
        var grandTotal = TaxCalculator.CalculateGrandTotal(subtotal, tax);

        // Robust timestamp-based order number (e.g. ORD-20260828143015-482)
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var randomSuffix = Random.Shared.Next(100, 999);
        var orderNumber = $"ORD-{timestamp}-{randomSuffix}";

        var order = new Order
        {
            OrderNumber = orderNumber,
            OrderDate = DateTimeOffset.UtcNow,
            Subtotal = subtotal,
            Tax = tax,
            GrandTotal = grandTotal,
            PaymentMethod = paymentMethod,
            Items = cartItems.Select(item => new OrderItem
            {
                ProductId = item.Product.Id,
                ProductName = item.Product.Name,
                UnitPrice = item.UnitPrice, // Historical price snapshot
                Quantity = item.Quantity,
                TotalPrice = item.TotalPrice
            }).ToList()
        };

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        return order;
    }

    public async Task<List<Order>> GetOrdersAsync()
    {
        var orders = await _dbContext.Orders
            .Include(o => o.Items)
            .AsNoTracking()
            .ToListAsync();

        return orders.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.Id).ToList();
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId)
    {
        return await _dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }
}
