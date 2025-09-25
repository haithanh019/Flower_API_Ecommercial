using System.ComponentModel.DataAnnotations;

namespace DataAccess.DTOs.ProductImageDTOs
{
    public class ProductImageDto
    {
        [Key]
        public int ProductImageId { get; set; }
        public int ProductId { get; set; }

        [Required, MaxLength(500)]
        public string ImageUrl { get; set; } = default!;
    }
}
