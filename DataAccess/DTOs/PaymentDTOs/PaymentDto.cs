using System.ComponentModel.DataAnnotations;
using Business.Entities; // PaymentMethod, PaymentStatus

namespace DataAccess.DTOs.PaymentDTOs
{
    public class PaymentDto
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? TransactionId { get; set; }
        public string? PaymentNote { get; set; }
    }
}
