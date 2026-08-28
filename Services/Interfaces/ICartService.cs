using CafePos.Models;

namespace CafePos.Services.Interfaces;

public interface ICartService
{
    IReadOnlyList<CartItem> Items { get; }
    decimal Subtotal { get; }
    decimal DiscountAmount { get; }
    decimal Tax { get; }
    decimal GrandTotal { get; }
    int TotalItemCount { get; }

    event EventHandler? CartChanged;

    void AddItem(Product product, int quantity = 1);
    void UpdateQuantity(int productId, int quantity);
    void RemoveItem(int productId);
    void ApplyDiscount(decimal percentage);
    void Clear();
}
