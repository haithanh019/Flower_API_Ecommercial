using System.Net.Http.Json;
using DataAccess.DTOs.ProductDTOs;
using DataAccess.Helper;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Client.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _http;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IHttpClientFactory http, ILogger<IndexModel> logger)
        {
            _http = http;
            _logger = logger;
        }

        public List<ProductDto> Products { get; set; } = new();
        public string? ErrorMessage { get; set; }

        // Hero stats
        public int TotalProducts { get; set; }
        public int TotalCustomers { get; set; } = 1000; // Static for now
        public string DeliveryTime { get; set; } = "24h";

        public async Task OnGetAsync()
        {
            await LoadFeaturedProductsAsync();
            await LoadStatsAsync();
        }

        private async Task LoadFeaturedProductsAsync()
        {
            try
            {
                var client = _http.CreateClient("odata");

                // Get featured products (top 8 products, active only)
                var url =
                    "Products?"
                    + "$filter=IsActive eq true"
                    + "&$orderby=ProductId desc"
                    + "&$top=8";

                _logger.LogInformation("Loading featured products from: {Url}", url);

                var response = await client.GetFromJsonAsync<OWrapper<ProductDto>>(url);

                if (response?.Value != null)
                {
                    Products = response.Value;
                    _logger.LogInformation("Loaded {Count} featured products", Products.Count);
                }
                else
                {
                    _logger.LogWarning("No products returned from API");
                    LoadFallbackProducts(); // Load fake data as fallback
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load featured products");
                LoadFallbackProducts(); // Load fake data as fallback
            }
        }

        private async Task LoadStatsAsync()
        {
            try
            {
                var client = _http.CreateClient("odata");

                // Get total products count
                var countResponse = await client.GetFromJsonAsync<OWrapper<ProductDto>>(
                    "Products?$filter=IsActive eq true&$count=true&$top=0"
                );

                if (countResponse != null)
                {
                    TotalProducts = countResponse.Count;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load stats");
                TotalProducts = 50; // Default fallback
            }
        }

        private void LoadFallbackProducts()
        {
            _logger.LogInformation("Loading fallback products");

            var fallbackProducts = new[]
            {
                new ProductDto
                {
                    ProductId = 1,
                    ProductName = "Hồng Phấn Dịu Dàng",
                    Price = 450000,
                    ImageUrls = new List<string> { "/images/flowers/rose-pink.jpg" },
                    ImageUrl = "/images/flowers/rose-pink.jpg",
                    IsActive = true,
                    Description = "Bó hồng phấn dịu dàng, thể hiện tình cảm chân thành",
                    CategoryId = 2,
                    StockQuantity = 10,
                },
                new ProductDto
                {
                    ProductId = 2,
                    ProductName = "Nắng Xuân Rực Rỡ",
                    Price = 520000,
                    ImageUrls = new List<string> { "/images/flowers/sunflower.jpg" },
                    ImageUrl = "/images/flowers/sunflower.jpg",
                    IsActive = true,
                    Description = "Hướng dương tươi mới, mang năng lượng tích cực",
                    CategoryId = 3,
                    StockQuantity = 8,
                },
                new ProductDto
                {
                    ProductId = 3,
                    ProductName = "Tulip Tinh Khôi",
                    Price = 380000,
                    ImageUrls = new List<string> { "/images/flowers/tulip-white.jpg" },
                    ImageUrl = "/images/flowers/tulip-white.jpg",
                    IsActive = true,
                    Description = "Tulip trắng tinh khôi, sang trọng và thanh lịch",
                    CategoryId = 2,
                    StockQuantity = 15,
                },
                new ProductDto
                {
                    ProductId = 4,
                    ProductName = "Lavender Mộng Mơ",
                    Price = 420000,
                    ImageUrls = new List<string> { "/images/flowers/lavender.jpg" },
                    ImageUrl = "/images/flowers/lavender.jpg",
                    IsActive = true,
                    Description = "Lavender thơm ngát, mang đến cảm giác thư giãn",
                    CategoryId = 1,
                    StockQuantity = 12,
                },
                new ProductDto
                {
                    ProductId = 5,
                    ProductName = "Mẫu Đơn Kiêu Sa",
                    Price = 680000,
                    ImageUrls = new List<string> { "/images/flowers/peony.jpg" },
                    ImageUrl = "/images/flowers/peony.jpg",
                    IsActive = true,
                    Description = "Mẫu đơn kiêu sa, biểu tượng của sự giàu có và vinh hoa",
                    CategoryId = 3,
                    StockQuantity = 5,
                },
                new ProductDto
                {
                    ProductId = 6,
                    ProductName = "Cúc Họa Mi Trắng",
                    Price = 320000,
                    ImageUrls = new List<string> { "/images/flowers/daisy-white.jpg" },
                    ImageUrl = "/images/flowers/daisy-white.jpg",
                    IsActive = true,
                    Description = "Cúc họa mi trắng tinh khôi, đơn giản mà đẹp",
                    CategoryId = 1,
                    StockQuantity = 20,
                },
                new ProductDto
                {
                    ProductId = 7,
                    ProductName = "Hướng Dương Rực Rỡ",
                    Price = 480000,
                    ImageUrls = new List<string> { "/images/flowers/sunflower-bright.jpg" },
                    ImageUrl = "/images/flowers/sunflower-bright.jpg",
                    IsActive = true,
                    Description = "Hướng dương rực rỡ, mang đến niềm vui và hy vọng",
                    CategoryId = 3,
                    StockQuantity = 7,
                },
                new ProductDto
                {
                    ProductId = 8,
                    ProductName = "Baby Breath Trắng",
                    Price = 280000,
                    ImageUrls = new List<string> { "/images/flowers/baby-breath.jpg" },
                    ImageUrl = "/images/flowers/baby-breath.jpg",
                    IsActive = true,
                    Description = "Baby breath trắng muốt, tinh tế và lãng mạn",
                    CategoryId = 2,
                    StockQuantity = 25,
                },
            };

            Products = fallbackProducts.ToList();
        }

        // Helper methods for view
        public string GetProductImage(ProductDto product)
        {
            return product.ImageUrl
                ?? product.ImageUrls?.FirstOrDefault()
                ?? "/images/flowers/placeholder.jpg";
        }

        public string GetProductBadge(ProductDto product)
        {
            // Simple logic to assign badges based on stock and ID
            if (product.StockQuantity <= 5)
                return "Hết hàng sớm";

            var id = product.ProductId;
            return (id % 3) switch
            {
                0 => "Mới",
                1 => "Bán Chạy",
                _ => "",
            };
        }

        public int GetProductRating(ProductDto product)
        {
            // Generate consistent rating based on product name hash
            var hash = Math.Abs(product.ProductName.GetHashCode());
            return (hash % 2) + 4; // Rating between 4-5
        }

        public decimal GetDisplayPrice(ProductDto product)
        {
            return product.Price;
        }

        public string FormatPrice(decimal price)
        {
            return price.ToString("N0") + " ₫";
        }

        // Categories for the category section
        public List<CategoryInfo> GetCategories()
        {
            return new List<CategoryInfo>
            {
                new CategoryInfo
                {
                    Id = 1,
                    Name = "Sinh Nhật",
                    Icon = "🎂",
                    Description = "Hoa tươi thắm để chúc mừng tuổi mới",
                    Slug = "birthday",
                },
                new CategoryInfo
                {
                    Id = 2,
                    Name = "Tình Yêu",
                    Icon = "💕",
                    Description = "Bày tỏ tình cảm với những bông hoa đẹp nhất",
                    Slug = "love",
                },
                new CategoryInfo
                {
                    Id = 3,
                    Name = "Chúc Mừng",
                    Icon = "🎉",
                    Description = "Thể hiện lời chúc mừng chân thành",
                    Slug = "congratulation",
                },
                new CategoryInfo
                {
                    Id = 4,
                    Name = "Tốt Nghiệp",
                    Icon = "🎓",
                    Description = "Chúc mừng thành công và bước sang trang mới",
                    Slug = "graduation",
                },
            };
        }

        // Features for the features section
        public List<FeatureInfo> GetFeatures()
        {
            return new List<FeatureInfo>
            {
                new FeatureInfo
                {
                    Icon = "fas fa-shipping-fast",
                    Title = "Giao Hàng Nhanh",
                    Description = "Giao trong ngày tại Đà Nẵng, đảm bảo hoa tươi và đúng giờ",
                },
                new FeatureInfo
                {
                    Icon = "fas fa-leaf",
                    Title = "Hoa Tươi Mỗi Ngày",
                    Description = "Nhập hoa tươi hàng ngày, đảm bảo chất lượng và độ bền cao",
                },
                new FeatureInfo
                {
                    Icon = "fas fa-edit",
                    Title = "Thiệp Viết Tay",
                    Description = "Thiệp được viết tay với nội dung cá nhân theo yêu cầu",
                },
                new FeatureInfo
                {
                    Icon = "fas fa-palette",
                    Title = "Thiết Kế Độc Đáo",
                    Description = "Các floral designer chuyên nghiệp tạo ra những bó hoa độc đáo",
                },
                new FeatureInfo
                {
                    Icon = "fas fa-gift",
                    Title = "Gói Quà Miễn Phí",
                    Description = "Gói quà đẹp mắt và professional hoàn toàn miễn phí",
                },
                new FeatureInfo
                {
                    Icon = "fas fa-headset",
                    Title = "Hỗ Trợ 24/7",
                    Description = "Đội ngũ chăm sóc khách hàng sẵn sàng hỗ trợ mọi lúc",
                },
            };
        }

        // Testimonials for the testimonials section
        public List<TestimonialInfo> GetTestimonials()
        {
            return new List<TestimonialInfo>
            {
                new TestimonialInfo
                {
                    Name = "Minh Tuấn",
                    Location = "Khách hàng thường xuyên",
                    Rating = 5,
                    Text =
                        "Hoa rất tươi và đẹp, giao đúng giờ. Bạn gái mình rất thích món quà này!",
                    Avatar = "/images/avatars/customer1.jpg",
                },
                new TestimonialInfo
                {
                    Name = "Thu Hằng",
                    Location = "Đà Nẵng",
                    Rating = 5,
                    Text =
                        "Dịch vụ tuyệt vời! Hoa đẹp và thiệp viết tay rất có tâm. Sẽ ủng hộ tiếp.",
                    Avatar = "/images/avatars/customer2.jpg",
                },
                new TestimonialInfo
                {
                    Name = "Văn Hải",
                    Location = "Hội An",
                    Rating = 5,
                    Text = "Đặt hoa online rất tiện, giao hàng nhanh và chính xác. Recommend!",
                    Avatar = "/images/avatars/customer3.jpg",
                },
            };
        }
    }

    // Helper classes
    public class CategoryInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
    }

    public class FeatureInfo
    {
        public string Icon { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class TestimonialInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int Rating { get; set; } = 5;
        public string Text { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
    }
}
