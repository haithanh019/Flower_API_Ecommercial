using AutoMapper;
using AutoMapper.QueryableExtensions;
using Business.Entities;
using DataAccess.DTOs.PaymentDTOs;
using Microsoft.EntityFrameworkCore;
using Repositories.UnitOfWork;
using Services.Interfaces;

namespace Services.Implements
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public PaymentService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<PaymentDto?> GetByIdAsync(int paymentId)
        {
            var entity = await _uow.PaymentRepository.GetByIdAsync(paymentId);
            return entity == null ? null : _mapper.Map<PaymentDto>(entity);
        }

        public async Task<PaymentDto?> GetByOrderIdAsync(int orderId) =>
            await _uow
                .PaymentRepository.Query()
                .Where(p => p.OrderId == orderId)
                .ProjectTo<PaymentDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();

        public async Task<int> CreateAsync(PaymentCreateDto dto)
        {
            // bảo toàn ràng buộc 1–1: nếu đã có payment cho Order thì không tạo trùng
            var existed = await _uow.PaymentRepository.GetByOrderIdAsync(dto.OrderId);
            if (existed != null)
                return existed.PaymentId;

            var entity = _mapper.Map<Payment>(dto);
            entity.Status = PaymentStatus.Pending;
            await _uow.PaymentRepository.AddAsync(entity);
            return entity.PaymentId;
        }

        public async Task<bool> UpdateAsync(PaymentUpdateDto dto)
        {
            var entity = await _uow.PaymentRepository.GetByIdAsync(dto.PaymentId);
            if (entity == null)
                return false;

            var prevStatus = entity.Status;
            _mapper.Map(dto, entity);

            // gợi ý: auto set PaymentDate nếu status đổi sang Completed
            if (prevStatus != PaymentStatus.Completed && entity.Status == PaymentStatus.Completed)
            {
                entity.PaymentDate = entity.PaymentDate ?? DateTime.UtcNow;
            }

            await _uow.PaymentRepository.UpdateAsync(entity);
            return true;
        }

        public async Task<bool> MarkCompletedAsync(
            int paymentId,
            string? transactionId = null,
            DateTime? paidAt = null
        )
        {
            var p = await _uow.PaymentRepository.GetByIdAsync(paymentId);
            if (p == null)
                return false;

            p.Status = PaymentStatus.Completed;
            p.TransactionId = transactionId ?? p.TransactionId;
            p.PaymentDate = paidAt ?? DateTime.UtcNow;

            await _uow.PaymentRepository.UpdateAsync(p);
            return true;
        }

        public async Task<bool> MarkFailedAsync(int paymentId, string? note = null)
        {
            var p = await _uow.PaymentRepository.GetByIdAsync(paymentId);
            if (p == null)
                return false;

            p.Status = PaymentStatus.Failed;
            p.PaymentNote = note ?? p.PaymentNote;

            await _uow.PaymentRepository.UpdateAsync(p);
            return true;
        }

        public async Task<bool> MarkRefundedAsync(int paymentId, string? note = null)
        {
            var p = await _uow.PaymentRepository.GetByIdAsync(paymentId);
            if (p == null)
                return false;

            p.Status = PaymentStatus.Refunded;
            p.PaymentNote = note ?? p.PaymentNote;

            await _uow.PaymentRepository.UpdateAsync(p);
            return true;
        }
    }
}
