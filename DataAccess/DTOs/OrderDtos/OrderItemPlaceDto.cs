using System.ComponentModel.DataAnnotations;

namespace DataAccess.DTOs.OrderDTOs;

public class OrderItemPlaceDto
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
