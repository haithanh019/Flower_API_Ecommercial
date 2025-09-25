using System.ComponentModel.DataAnnotations;

namespace DataAccess.DTOs.ProductDTOs
{
    public class ProductDto
    {
        [Key]
        public int ProductId { get; set; }

        [Required, MaxLength(200)]
        public string ProductName { get; set; } = default!;

        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }

        public bool IsActive { get; set; }

        // Ảnh đại diện (lấy ảnh đầu tiên)
        public string? ImageUrl { get; set; }

        // Danh sách tất cả ảnh
        public List<string> ImageUrls { get; set; } = new();
    }
}
