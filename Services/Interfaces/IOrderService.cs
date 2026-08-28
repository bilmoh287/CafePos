using CafePos.Models;

namespace CafePos.Services.Interfaces;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(IReadOnlyList<CartItem> cartItems, PaymentMethod paymentMethod);
    Task<List<Order>> GetOrdersAsync();
    Task<Order?> GetOrderByIdAsync(int orderId);
}
