namespace CafePos.Models;

public class CartItem
{
    public Product Product { get; }
    public int Quantity { get; set; }

    public decimal UnitPrice => Product.Price;
    public decimal TotalPrice => UnitPrice * Quantity;

    public CartItem(Product product, int quantity = 1)
    {
        Product = product ?? throw new ArgumentNullException(nameof(product));
        Quantity = quantity > 0 ? quantity : 1;
    }
}
