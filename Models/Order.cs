namespace CafePos.Models;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.Now;
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal GrandTotal { get; set; }
    public PaymentMethod PaymentMethod { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}
