namespace Business.Entities;

public class OrderItem
{
    public int OrderItemId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;

    // Foreign keys
    public int OrderId { get; set; }
    public int ProductId { get; set; }

    // Navigation properties
    public Order Order { get; set; } = default!;
    public Product Product { get; set; } = default!;
}
