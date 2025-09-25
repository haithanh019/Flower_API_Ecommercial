using System.ComponentModel.DataAnnotations;
using Business.Entities; // UserRole

namespace DataAccess.DTOs.UserDTOs
{
    public class UserUpdateDto
    {
        [Required, MaxLength(120)]
        public string FullName { get; set; } = default!;

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = default!;

        [Required, MaxLength(20)]
        public string PhoneNumber { get; set; } = default!;

        [Required, MaxLength(300)]
        public string Address { get; set; } = default!;

        public UserRole Role { get; set; } = UserRole.Customer;

        public bool IsActive { get; set; } = true;
    }
}
