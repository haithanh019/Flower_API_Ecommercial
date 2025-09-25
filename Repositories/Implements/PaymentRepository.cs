using System.Threading.Tasks;
using Business.Entities;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories.Implements
{
    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        public PaymentRepository(FlowerShopDbContext db)
            : base(db) { }

        public Task<Payment?> GetByOrderIdAsync(int orderId) =>
            _db.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.OrderId == orderId);
    }
}
