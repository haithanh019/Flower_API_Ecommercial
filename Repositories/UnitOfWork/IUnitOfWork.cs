using Repositories.Interfaces;

namespace Repositories.UnitOfWork
{
    public interface IUnitOfWork
    {
        IOrderRepository OrderRepository { get; }
        IProductRepository ProductRepository { get; }
        IUserRepository UserRepository { get; }
        ICategoryRepository CategoryRepository { get; }
        IPaymentRepository PaymentRepository { get; }
        IProductImageRepository ProductImageRepository { get; }
        ICartRepository CartRepository { get; }
        ICartItemRepository CartItemRepository { get; }
        Task SaveAsync();
    }
}
