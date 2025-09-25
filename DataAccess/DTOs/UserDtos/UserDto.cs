using System.ComponentModel.DataAnnotations;
using Business.Entities;

namespace DataAccess.DTOs.UserDTOs
{
    public class UserDto
    {
        [Key]
        public int UserId { get; set; }

        [Required, MaxLength(120)]
        public string FullName { get; set; } = default!;

        [Required, MaxLength(20)]
        public string PhoneNumber { get; set; } = default!;

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = default!;

        [Required, MaxLength(300)]
        public string Address { get; set; } = default!;

        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
    }
}
