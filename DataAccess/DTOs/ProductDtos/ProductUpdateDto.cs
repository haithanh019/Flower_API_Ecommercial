using System.ComponentModel.DataAnnotations;

namespace DataAccess.DTOs.ProductDTOs
{
    public class ProductUpdateDto
    {
        [Required, MaxLength(200)]
        public string ProductName { get; set; } = default!;

        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }

        public bool IsActive { get; set; } = true;

        // Danh sách ảnh mong muốn sau khi cập nhật (server sẽ reconcile)
        public List<string> ImageUrls { get; set; } = new();
    }
}
