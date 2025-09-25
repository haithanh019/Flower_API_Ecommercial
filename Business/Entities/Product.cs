namespace Business.Entities;

public class Product : BaseEntity
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    // Foreign key
    public int CategoryId { get; set; }

    // Navigation properties
    public Category Category { get; set; } = default!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}
