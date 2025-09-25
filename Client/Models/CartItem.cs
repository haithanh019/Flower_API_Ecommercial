namespace Client.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int CategoryId { get; set; }
        public string? ImageUrl { get; set; }
    }
}
