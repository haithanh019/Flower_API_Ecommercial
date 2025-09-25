namespace Business.Entities;

public class Category : BaseEntity
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
