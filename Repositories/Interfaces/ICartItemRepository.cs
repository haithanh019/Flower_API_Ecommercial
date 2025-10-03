using Business.Entities;

namespace Repositories.Interfaces
{
    public interface ICartItemRepository : IRepository<CartItem>
    {
        Task<CartItem?> GetByIdWithCartAsync(int cartItemId);
        Task<CartItem?> FindByCartAndProductAsync(int cartId, int productId);
        Task ClearByCartIdAsync(int cartId);
    }
}
