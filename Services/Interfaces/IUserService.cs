using DataAccess.DTOs.UserDTOs;

namespace Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> LoginAsync(LoginDTO loginDto);
    }
}
