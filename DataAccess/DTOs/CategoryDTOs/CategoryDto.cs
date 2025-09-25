using System.ComponentModel.DataAnnotations;

namespace DataAccess.DTOs.CategoryDTOs
{
    public class CategoryDto
    {
        [Key]
        public int CategoryId { get; set; }

        [Required, MaxLength(150)]
        public string CategoryName { get; set; } = default!;

        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
    }
}
