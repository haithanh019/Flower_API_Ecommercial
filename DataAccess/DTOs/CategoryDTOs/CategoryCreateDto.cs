using System.ComponentModel.DataAnnotations;

namespace DataAccess.DTOs.CategoryDTOs
{
    public class CategoryCreateDto
    {
        [Required, MaxLength(150)]
        public string CategoryName { get; set; } = default!;

        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
