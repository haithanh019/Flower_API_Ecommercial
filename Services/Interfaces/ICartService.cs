using DataAccess.DTOs.CartDTOs;

namespace Services.Interfaces
{
    public interface ICartService
    {
        Task<CartDto> GetOrCreateAsync(int userId);
        Task<CartDto> AddItemAsync(int userId, int productId, int quantity);
        Task<CartDto> UpdateItemQtyAsync(int userId, int cartItemId, int quantity);
        Task<bool> RemoveItemAsync(int userId, int cartItemId);
        Task<CartDto> ClearAsync(int userId);
    }
}
