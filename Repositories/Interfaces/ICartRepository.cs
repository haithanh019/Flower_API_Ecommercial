using Business.Entities;

namespace Repositories.Interfaces
{
    public interface ICartRepository : IRepository<Cart>
    {
        Task<Cart?> GetByUserIdAsync(int userId, bool includeDetails = true);
        Task<Cart> GetOrCreateByUserIdAsync(int userId);
    }
}
