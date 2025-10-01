namespace Business.Entities;

public class Cart : BaseEntity
{
    public int CartId { get; set; }
    public int UserId { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
