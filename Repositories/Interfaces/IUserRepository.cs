using Business.Entities;

namespace Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAndPasswordAsync(string email, string password);
    }
}
