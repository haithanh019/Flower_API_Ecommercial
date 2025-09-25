using Business.Entities;
using DataAccess;
using Repositories.Interfaces;

namespace Repositories.Implements;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(FlowerShopDbContext db)
        : base(db) { }
}
