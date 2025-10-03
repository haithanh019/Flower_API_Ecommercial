using System.ComponentModel.DataAnnotations;

namespace DataAccess.DTOs.CartDTOs
{
    public class CartDto
    {
        [Key]
        public int CartId { get; set; }
        public int UserId { get; set; }
        public int ItemCount { get; set; }
        public decimal Subtotal { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CartItemDto
    {
        [Key]
        public int CartItemId { get; set; }
        public int CartId { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public string? ImageUrl { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => Quantity * UnitPrice;
    }

    // Action bodies
    public class CartAddItemRequest
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;
    }

    public class CartUpdateQtyRequest
    {
        [Range(1, int.MaxValue)]
        public int CartItemId { get; set; }

        // <=0 sẽ xóa item
        public int Quantity { get; set; }
    }

    public class CartRemoveItemRequest
    {
        [Range(1, int.MaxValue)]
        public int CartItemId { get; set; }
    }
}
