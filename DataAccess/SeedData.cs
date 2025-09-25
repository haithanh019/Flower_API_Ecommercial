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
                    PasswordHash = "hash2",
                    Address = "TP.HCM",
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
                    Description = "Hoa cúc trắng",
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
            };

            context.Products.AddRange(products);
            context.SaveChanges(); // Để ProductId được sinh ra

            // 4. Seed Orders
            var orders = new List<Order>
            {
                new Order
                {
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Delivered,
                    TotalAmount = 800000,
                    ShippingAddress = "28 Trần Duy Hưng, Hà Nội",
                    CustomerId = users[1].UserId,
                    CustomerNote = "Giao trong giờ hành chính",
                    IsDeleted = false,
                },
            };
            context.Orders.AddRange(orders);
            context.SaveChanges(); // Để OrderId được sinh ra

            // 5. Seed OrderItems
            var orderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    Quantity = 1,
                    UnitPrice = products[0].Price,
                    OrderId = orders[0].OrderId,
                    ProductId = products[0].ProductId,
                },
                new OrderItem
                {
                    Quantity = 1,
                    UnitPrice = products[1].Price,
                    OrderId = orders[0].OrderId,
                    ProductId = products[1].ProductId,
                },
            };
            context.OrderItems.AddRange(orderItems);
            context.SaveChanges();

            // 6. Seed Payment
            var payment = new Payment
            {
                Amount = 800000,
                Method = PaymentMethod.BankTransfer,
                Status = PaymentStatus.Completed,
                PaymentDate = DateTime.UtcNow,
                TransactionId = "TXN001",
                PaymentNote = "Đã chuyển khoản",
                OrderId = orders[0].OrderId,
            };
            context.Payments.Add(payment);
            context.SaveChanges();
        }
    }
}
