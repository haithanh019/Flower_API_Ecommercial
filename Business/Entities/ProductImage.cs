namespace Business.Entities
{
    public class ProductImage : BaseEntity
    {
        public int ProductImageId { get; set; }

        // Đường dẫn hoặc URL hình ảnh
        public string ImageUrl { get; set; } = default!;

        // FK tới Product
        public int ProductId { get; set; }
        public Product Product { get; set; } = default!;
    }
}
