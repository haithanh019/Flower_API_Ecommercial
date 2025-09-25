using System.ComponentModel.DataAnnotations;
using Business.Entities;

namespace DataAccess.DTOs.OrderDTOs
{
    public class OrderPlaceDto
    {
        // Sẽ bị override bằng userId của token nếu vai trò là Customer
        [Required]
        public int CustomerId { get; set; }

        [Required, MaxLength(300)]
        public string ShippingAddress { get; set; } = default!;

        [MaxLength(500)]
        public string? CustomerNote { get; set; }

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        [MinLength(1)]
        public List<OrderItemPlaceDto> Items { get; set; } = new();
    }
}
