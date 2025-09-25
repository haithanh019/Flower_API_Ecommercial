using Business.Entities;
using DataAccess;
using Repositories.Interfaces;

namespace Repositories.Implements;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(FlowerShopDbContext db)
        : base(db) { }
}
