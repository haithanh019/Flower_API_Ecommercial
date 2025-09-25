using Repositories.Implements;
using Repositories.Interfaces;
using Repositories.UnitOfWork;

namespace DataAccess.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FlowerShopDbContext _db;

        public IOrderRepository OrderRepository { get; }
        public IProductRepository ProductRepository { get; }
        public IUserRepository UserRepository { get; }
        public ICategoryRepository CategoryRepository { get; }
        public IPaymentRepository PaymentRepository { get; }
        public IProductImageRepository ProductImageRepository { get; private set; }

        public UnitOfWork(FlowerShopDbContext db)
        {
            _db = db;
            OrderRepository = new OrderRepository(_db);
            ProductRepository = new ProductRepository(_db);
            UserRepository = new UserRepository(_db);
            CategoryRepository = new CategoryRepository(_db);
            PaymentRepository = new PaymentRepository(_db);
            ProductImageRepository = new ProductImageRepository(_db);
        }

        public Task SaveAsync() => _db.SaveChangesAsync();
    }
}
