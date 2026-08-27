using CafePos.Helpers;
using CafePos.Models;
using CafePos.Services.Interfaces;

namespace CafePos.Services.Implementations;

public class CartService : ICartService
{
    private readonly List<CartItem> _items = new();

    public IReadOnlyList<CartItem> Items => _items.AsReadOnly();

    public decimal Subtotal => _items.Sum(i => i.TotalPrice);

    public decimal Tax => TaxCalculator.CalculateTax(Subtotal);

    public decimal GrandTotal => TaxCalculator.CalculateGrandTotal(Subtotal, Tax);

    public int TotalItemCount => _items.Sum(i => i.Quantity);

    public event EventHandler? CartChanged;

    public void AddItem(Product product, int quantity = 1)
    {
        if (product == null) return;
        if (quantity <= 0) return;

        var existingItem = _items.FirstOrDefault(i => i.Product.Id == product.Id);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            _items.Add(new CartItem(product, quantity));
        }

        NotifyCartChanged();
    }

    public void UpdateQuantity(int productId, int quantity)
    {
        if (quantity <= 0)
        {
            RemoveItem(productId);
            return;
        }

        var existingItem = _items.FirstOrDefault(i => i.Product.Id == productId);
        if (existingItem != null)
        {
            existingItem.Quantity = quantity;
            NotifyCartChanged();
        }
    }

    public void RemoveItem(int productId)
    {
        int countBefore = _items.Count;
        _items.RemoveAll(i => i.Product.Id == productId);

        if (_items.Count != countBefore)
        {
            NotifyCartChanged();
        }
    }

    public void Clear()
    {
        if (_items.Count > 0)
        {
            _items.Clear();
            NotifyCartChanged();
        }
    }

    private void NotifyCartChanged()
    {
        CartChanged?.Invoke(this, EventArgs.Empty);
    }
}
