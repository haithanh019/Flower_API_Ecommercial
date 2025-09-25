using Business.Entities;
using DataAccess;
using Repositories.Interfaces;

namespace Repositories.Implements
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(FlowerShopDbContext db)
            : base(db) { }
    }
}
