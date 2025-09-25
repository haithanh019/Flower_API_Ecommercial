using System.ComponentModel.DataAnnotations;

namespace DataAccess.DTOs.OrderDTOs
{
    public class OrderDto
    {
        [Key]
        public int OrderId { get; set; }

        public int CustomerId { get; set; }
        public string? CustomerName { get; set; } // NEW (map từ User.FullName)

        public DateTime OrderDate { get; set; }

        public int Status { get; set; } // map enum -> int

        public string ShippingAddress { get; set; } = default!;
        public string? CustomerNote { get; set; }

        public decimal TotalAmount { get; set; }

        // Payment (đặt nullable để an toàn)
        public int? PaymentStatus { get; set; } // 0 Pending, 1 Completed, 2 Failed, 3 Refunded
        public int? PaymentMethod { get; set; } // 0 Cash, 1 BankTransfer, 2 EWallet
        public DateTime? PaymentDate { get; set; }
        public string? TransactionId { get; set; }

        // Chi tiết
        public List<OrderItemDto> Items { get; set; } = new(); // NEW
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => Quantity * UnitPrice;

        public string? ImageUrl { get; set; }
    }
}
