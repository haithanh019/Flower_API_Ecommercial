namespace Business.Entities;

public class User : BaseEntity
{
    public int UserId { get; set; }
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string Address { get; set; } = default!;
    public UserRole Role { get; set; } = UserRole.Customer;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

public enum UserRole
{
    Customer = 0,
    Admin = 1,
}
