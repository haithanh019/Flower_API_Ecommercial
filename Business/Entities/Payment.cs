using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Entities
{
    public class Payment : BaseEntity
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; } = PaymentMethod.Cash;
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public DateTime? PaymentDate { get; set; }
        public string? TransactionId { get; set; }
        public string? PaymentNote { get; set; }

        // Foreign key
        public int OrderId { get; set; }

        // Navigation property
        public Order Order { get; set; } = default!;
    }

    public enum PaymentMethod
    {
        Cash = 0, // Tiền mặt
        BankTransfer = 1, // Chuyển khoản ngân hàng
        EWallet = 2, // Ví điện tử (Momo, ZaloPay...)
    }

    public enum PaymentStatus
    {
        Pending = 0, // Chờ thanh toán
        Completed = 1, // Đã thanh toán
        Failed = 2, // Thanh toán thất bại
        Refunded = 3, // Đã hoàn tiền
    }
}
