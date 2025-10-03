namespace Business.Entities;

public class CartItem : BaseEntity
{
    public int CartItemId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } // Giá tại thời điểm thêm vào giỏ
    public decimal TotalPrice => Quantity * UnitPrice;

    // Foreign keys
    public int CartId { get; set; }
    public int ProductId { get; set; }

    // Navigation properties
    public Cart Cart { get; set; } = default!;
    public Product Product { get; set; } = default!;
}
