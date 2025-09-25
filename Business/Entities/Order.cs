namespace Business.Entities;

public class Order : BaseEntity
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }
    public string ShippingAddress { get; set; } = default!;
    public string? CustomerNote { get; set; }

    // Foreign key
    public int CustomerId { get; set; }

    // Navigation properties
    public User Customer { get; set; } = default!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public Payment? Payment { get; set; }
}

public enum OrderStatus
{
    Pending = 0, // Đang chờ xử lý
    Confirmed = 1, // Đã xác nhận
    Processing = 2, // Đang chuẩn bị
    Shipping = 3, // Đang giao hàng
    Delivered = 4, // Đã giao hàng
    Cancelled = 5, // Đã hủy
}
