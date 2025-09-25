using Business.Entities;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories.Implements
{
    public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
    {
        public ProductImageRepository(FlowerShopDbContext db)
            : base(db) { }

        public Task<List<ProductImage>> GetByProductIdAsync(int productId) =>
            _db.ProductImages.Where(x => x.ProductId == productId).ToListAsync();
    }
}
