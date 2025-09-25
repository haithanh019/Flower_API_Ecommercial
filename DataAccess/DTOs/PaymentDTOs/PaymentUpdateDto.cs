using System.ComponentModel.DataAnnotations;
using Business.Entities; // PaymentStatus, PaymentMethod

namespace DataAccess.DTOs.PaymentDTOs
{
    public class PaymentUpdateDto
    {
        [Required]
        public int PaymentId { get; set; }

        // Cho phép chỉnh khi có điều chỉnh từ cổng thanh toán
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }

        public DateTime? PaymentDate { get; set; }
        public string? TransactionId { get; set; }
        public string? PaymentNote { get; set; }
    }
}
