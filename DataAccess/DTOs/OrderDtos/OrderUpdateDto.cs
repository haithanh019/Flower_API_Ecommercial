using System.ComponentModel.DataAnnotations;
using Business.Entities;

namespace DataAccess.DTOs.OrderDTOs
{
    public class OrderUpdateDto
    {
        public OrderStatus Status { get; set; }

        [MaxLength(300)]
        public string? ShippingAddress { get; set; }

        [MaxLength(500)]
        public string? CustomerNote { get; set; }
    }
}
