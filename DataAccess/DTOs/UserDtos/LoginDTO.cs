using System.ComponentModel.DataAnnotations;

namespace DataAccess.DTOs.UserDTOs
{
    public class LoginDTO
    {
        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = default!;

        [Required, MinLength(6), MaxLength(100)]
        public string Password { get; set; } = default!;
    }
}
