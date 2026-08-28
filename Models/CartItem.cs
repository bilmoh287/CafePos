using CommunityToolkit.Mvvm.ComponentModel;

namespace CafePos.Models;

public partial class CartItem : ObservableObject
{
    public Product Product { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPrice))]
    public partial int Quantity { get; set; }

    public decimal UnitPrice => Product.Price;
    public decimal TotalPrice => UnitPrice * Quantity;

    public CartItem(Product product, int quantity = 1)
    {
        Product = product ?? throw new ArgumentNullException(nameof(product));
        Quantity = quantity > 0 ? quantity : 1;
    }
}
