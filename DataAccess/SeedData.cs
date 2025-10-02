using Business.Entities;

namespace DataAccess
{
    public static class SeedData
    {
        public static void Initialize(FlowerShopDbContext context)
        {
            // Nếu đã có dữ liệu rồi thì không seed lại
            if (context.Users.Any())
                return;

            // 1. Seed Users
            var users = new List<User>
            {
                new User
                {
                    FullName = "Hoàng Hải Thành",
                    Email = "haithanh019@gmail.com",
                    PhoneNumber = "0868060603",
                    PasswordHash = "Thanh0965324682",
                    Address = "Quảng Bình",
                    Role = UserRole.Admin,
                    IsActive = true,
                },
                new User
                {
                    FullName = "Trần Thị B",
                    Email = "b@example.com",
                    PhoneNumber = "0900000222",
                    PasswordHash = "hash1234",
                    Address = "TP.HCM",
                    Role = UserRole.Customer,
                    IsActive = true,
                },
                new User
                {
                    FullName = "Nguyễn Văn C",
                    Email = "c@example.com",
                    PhoneNumber = "0900000333",
                    PasswordHash = "hash1234",
                    Address = "Hà Nội",
                    Role = UserRole.Customer,
                    IsActive = true,
                },
            };
            context.Users.AddRange(users);

            // 2. Seed Categories
            var categories = new List<Category>
            {
                new Category
                {
                    CategoryName = "Hoa Hồng",
                    Description = "Các sản phẩm hoa hồng",
                    IsActive = true,
                },
                new Category
                {
                    CategoryName = "Hoa Cúc",
                    Description = "Các sản phẩm hoa cúc",
                    IsActive = true,
                },
                new Category
                {
                    CategoryName = "Hoa Ly",
                    Description = "Các sản phẩm hoa ly",
                    IsActive = true,
                },
            };
            context.Categories.AddRange(categories);
            context.SaveChanges(); // Để CategoryId được sinh ra

            // 3. Seed Products
            var products = new List<Product>
            {
                new Product
                {
                    ProductName = "Bó Hoa Hồng Đỏ",
                    Description = "Hoa hồng đỏ tươi",
                    Price = 450000,
                    StockQuantity = 15,
                    IsActive = true,
                    CategoryId = categories[0].CategoryId,
                    ProductImages = new List<ProductImage>
                    {
                        new ProductImage
                        {
                            ImageUrl = "https://localhost:7119/uploads/products/bo-hoa-hong-do.jpg",
                        },
                        new ProductImage
                        {
                            ImageUrl =
                                "https://localhost:7119/uploads/products/bo-hoa-hong-do-1.jpg",
                        },
                    },
                },
                new Product
                {
                    ProductName = "Giỏ Hoa Cúc Trắng",
                    Description = "Hoa cúc trắng tinh khiết",
                    Price = 350000,
                    StockQuantity = 10,
                    IsActive = true,
                    CategoryId = categories[1].CategoryId,
                    ProductImages = new List<ProductImage>
                    {
                        new ProductImage
                        {
                            ImageUrl =
                                "https://localhost:7119/uploads/products/gio-hoa-cuc-trang.jpg",
                        },
                    },
                },
                new Product
                {
                    ProductName = "Hoa Ly Vàng",
                    Description = "Hoa ly vàng thơm nồng",
                    Price = 280000,
                    StockQuantity = 8,
                    IsActive = true,
                    CategoryId = categories[2].CategoryId,
                    ProductImages = new List<ProductImage>
                    {
                        new ProductImage
                        {
                            ImageUrl = "https://localhost:7119/uploads/products/hoa-ly-vang.jpg",
                        },
                    },
                },
                new Product
                {
                    ProductName = "Bó Hoa Hồng Trắng",
                    Description = "Hoa hồng trắng tinh tế",
                    Price = 420000,
                    StockQuantity = 12,
                    IsActive = true,
                    CategoryId = categories[0].CategoryId,
                    ProductImages = new List<ProductImage>
                    {
                        new ProductImage
                        {
                            ImageUrl =
                                "https://localhost:7119/uploads/products/bo-hoa-hong-trang.jpg",
                        },
                    },
                },
            };
            context.Products.AddRange(products);
            context.SaveChanges(); // Để ProductId được sinh ra

            // 4. Seed Carts (Active shopping carts)
            var carts = new List<Cart>
            {
                // Cart cho customer Trần Thị B (có items)
                new Cart
                {
                    UserId = users[1].UserId, // Trần Thị B
                },
                // Cart trống cho customer Nguyễn Văn C
                new Cart
                {
                    UserId = users[2].UserId, // Nguyễn Văn C
                },
            };
            context.Carts.AddRange(carts);
            context.SaveChanges(); // Để CartId được sinh ra

            // 5. Seed CartItems
            var cartItems = new List<CartItem>
            {
                // Items trong cart của Trần Thị B
                new CartItem
                {
                    CartId = carts[0].CartId,
                    ProductId = products[0].ProductId, // Bó Hoa Hồng Đỏ
                    Quantity = 2,
                    UnitPrice = products[0].Price, // Giá tại thời điểm thêm vào cart
                },
                new CartItem
                {
                    CartId = carts[0].CartId,
                    ProductId = products[2].ProductId, // Hoa Ly Vàng
                    Quantity = 1,
                    UnitPrice = products[2].Price,
                },
            };
            context.CartItems.AddRange(cartItems);
            context.SaveChanges();

            // 6. Seed Orders (Completed orders)
            var orders = new List<Order>
            {
                new Order
                {
                    OrderDate = DateTime.UtcNow.AddDays(-5), // 5 ngày trước
                    Status = OrderStatus.Delivered,
                    TotalAmount = 800000,
                    ShippingAddress = "28 Trần Duy Hưng, Hà Nội",
                    CustomerId = users[1].UserId,
                    CustomerNote = "Giao trong giờ hành chính",
                    IsDeleted = false,
                },
                new Order
                {
                    OrderDate = DateTime.UtcNow.AddDays(-2), // 2 ngày trước
                    Status = OrderStatus.Processing,
                    TotalAmount = 720000,
                    ShippingAddress = "123 Nguyễn Huệ, TP.HCM",
                    CustomerId = users[2].UserId,
                    CustomerNote = "Giao vào buổi sáng",
                    IsDeleted = false,
                },
            };
            context.Orders.AddRange(orders);
            context.SaveChanges(); // Để OrderId được sinh ra

            // 7. Seed OrderItems
            var orderItems = new List<OrderItem>
            {
                // Items trong order đầu tiên
                new OrderItem
                {
                    Quantity = 1,
                    UnitPrice = products[0].Price, // Bó Hoa Hồng Đỏ
                    OrderId = orders[0].OrderId,
                    ProductId = products[0].ProductId,
                },
                new OrderItem
                {
                    Quantity = 1,
                    UnitPrice = products[1].Price, // Giỏ Hoa Cúc Trắng
                    OrderId = orders[0].OrderId,
                    ProductId = products[1].ProductId,
                },
                // Items trong order thứ hai
                new OrderItem
                {
                    Quantity = 2,
                    UnitPrice = products[2].Price, // Hoa Ly Vàng
                    OrderId = orders[1].OrderId,
                    ProductId = products[2].ProductId,
                },
                new OrderItem
                {
                    Quantity = 1,
                    UnitPrice = 160000, // Giá khuyến mãi tại thời điểm đó
                    OrderId = orders[1].OrderId,
                    ProductId = products[3].ProductId, // Bó Hoa Hồng Trắng
                },
            };
            context.OrderItems.AddRange(orderItems);
            context.SaveChanges();

            // 8. Seed Payments
            var payments = new List<Payment>
            {
                new Payment
                {
                    Amount = 800000,
                    Method = PaymentMethod.BankTransfer,
                    Status = PaymentStatus.Completed,
                    PaymentDate = DateTime.UtcNow.AddDays(-5),
                    TransactionId = "TXN001",
                    PaymentNote = "Đã chuyển khoản",
                    OrderId = orders[0].OrderId,
                },
                new Payment
                {
                    Amount = 720000,
                    Method = PaymentMethod.EWallet,
                    Status = PaymentStatus.Completed,
                    PaymentDate = DateTime.UtcNow.AddDays(-2),
                    TransactionId = "MOMO_TXN002",
                    PaymentNote = "Thanh toán qua MoMo",
                    OrderId = orders[1].OrderId,
                },
            };
            context.Payments.AddRange(payments);
            context.SaveChanges();
        }
    }
}
