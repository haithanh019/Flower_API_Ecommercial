using Business.Entities;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories.Implements
{
    public class CartRepository : Repository<Cart>, ICartRepository
    {
        public CartRepository(FlowerShopDbContext db)
            : base(db) { }

        public async Task<Cart?> GetByUserIdAsync(int userId, bool includeDetails = true)
        {
            var q = _db.Carts.AsQueryable();
            if (includeDetails)
            {
                q = q.Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p.ProductImages);
            }
            return await q.SingleOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<Cart> GetOrCreateByUserIdAsync(int userId)
        {
            var cart = await GetByUserIdAsync(userId, includeDetails: true);
            if (cart != null)
                return cart;

            cart = new Cart { UserId = userId };
            await _db.Carts.AddAsync(cart);
            await _db.SaveChangesAsync();
            // nạp lại kèm details rỗng để mapper dùng
            return await GetByUserIdAsync(userId, includeDetails: true) ?? cart;
        }
    }
}
