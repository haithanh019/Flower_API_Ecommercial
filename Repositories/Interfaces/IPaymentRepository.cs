using Business.Entities;

namespace Repositories.Interfaces
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<Payment?> GetByOrderIdAsync(int orderId);
    }
}
