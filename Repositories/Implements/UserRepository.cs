using Business.Entities;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories.Implements
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(FlowerShopDbContext context)
            : base(context) { }

        public async Task<User?> GetByEmailAndPasswordAsync(string email, string password)
        {
            return await _db.Users.FirstOrDefaultAsync(u =>
                u.Email == email && u.PasswordHash == password
            );
        }
    }
}
