using DataAccess.DTOs.UserDTOs;

namespace DataAccess.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = new UserDto { Email = string.Empty };
    }
}
