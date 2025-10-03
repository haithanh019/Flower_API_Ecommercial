using Business.Entities;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories.Implements
{
    public class CartItemRepository : Repository<CartItem>, ICartItemRepository
    {
        public CartItemRepository(FlowerShopDbContext db)
            : base(db) { }

        public Task<CartItem?> GetByIdWithCartAsync(int cartItemId) =>
            _db
                .CartItems.Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);

        public Task<CartItem?> FindByCartAndProductAsync(int cartId, int productId) =>
            _db.CartItems.FirstOrDefaultAsync(x => x.CartId == cartId && x.ProductId == productId);

        public async Task ClearByCartIdAsync(int cartId)
        {
            var items = await _db.CartItems.Where(ci => ci.CartId == cartId).ToListAsync();
            _db.CartItems.RemoveRange(items);
            await _db.SaveChangesAsync();
        }
    }
}
