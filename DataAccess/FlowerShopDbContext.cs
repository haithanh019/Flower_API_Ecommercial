using Business.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class FlowerShopDbContext : DbContext
{
    public FlowerShopDbContext(DbContextOptions<FlowerShopDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.UserId);
            e.Property(x => x.FullName).HasMaxLength(120).IsRequired();
            e.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200);
            e.Property(x => x.Address).HasMaxLength(300).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(100).IsRequired();
            e.Property(x => x.Role).IsRequired();
        });

        // Category
        modelBuilder.Entity<Category>(e =>
        {
            e.HasKey(x => x.CategoryId);
            e.Property(x => x.CategoryName).HasMaxLength(150).IsRequired();
        });

        // Product
        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(x => x.ProductId);
            e.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");

            // Quan hệ 1-n Product - ProductImages
            e.HasMany(x => x.ProductImages)
                .WithOne(c => c.Product)
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ProductImage
        modelBuilder.Entity<ProductImage>(e =>
        {
            e.HasKey(pi => pi.ProductImageId);
            e.Property(pi => pi.ImageUrl).IsRequired().HasMaxLength(500);
            e.HasOne(pi => pi.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        // Order
        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(x => x.OrderId);
            e.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.ShippingAddress).HasMaxLength(300).IsRequired();
            e.Property(x => x.Status).IsRequired();

            e.HasOne(o => o.Customer)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(o => o.Payment)
                .WithOne(p => p.Order)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // OrderItem
        modelBuilder.Entity<OrderItem>(e =>
        {
            e.HasKey(x => x.OrderItemId);
            e.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");

            e.HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Payment
        modelBuilder.Entity<Payment>(e =>
        {
            e.HasKey(p => p.PaymentId);
            e.Property(p => p.Amount).HasColumnType("decimal(18,2)").IsRequired();
            e.Property(p => p.Method).IsRequired();
            e.Property(p => p.Status).IsRequired();
            e.Property(p => p.PaymentDate).HasDefaultValueSql("GETDATE()");
        });

        base.OnModelCreating(modelBuilder);
    }
}
