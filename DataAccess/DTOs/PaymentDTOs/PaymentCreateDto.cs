using System.ComponentModel.DataAnnotations;
using Business.Entities;

namespace DataAccess.DTOs.PaymentDTOs
{
    public class PaymentCreateDto
    {
        [Required]
        public int OrderId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        public PaymentMethod Method { get; set; } = PaymentMethod.Cash;

        public string? TransactionId { get; set; }
        public string? PaymentNote { get; set; }
    }
}
