using System.ComponentModel.DataAnnotations;
using Business.Entities;

namespace DataAccess.DTOs.UserDTOs
{
    public class UserCreateDto
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

        // Mật khẩu thô để service tự hash
        [Required, MaxLength(100)]
        public string Password { get; set; } = default!;
    }
}
