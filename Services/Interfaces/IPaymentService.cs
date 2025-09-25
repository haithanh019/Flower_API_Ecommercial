using DataAccess.DTOs.PaymentDTOs;

namespace Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentDto?> GetByIdAsync(int paymentId);
        Task<PaymentDto?> GetByOrderIdAsync(int orderId);

        Task<int> CreateAsync(PaymentCreateDto dto);
        Task<bool> UpdateAsync(PaymentUpdateDto dto);

        // Tiện ích thường dùng
        Task<bool> MarkCompletedAsync(
            int paymentId,
            string? transactionId = null,
            DateTime? paidAt = null
        );
        Task<bool> MarkFailedAsync(int paymentId, string? note = null);
        Task<bool> MarkRefundedAsync(int paymentId, string? note = null);
    }
}
