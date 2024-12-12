namespace VegaCityApp.API.Payload.Request.Store
{
    public class CreateProductRequest
    {
        public string Name { get; set; } = null!;
        public Guid ProductCategoryId { get; set; }
        public int Price { get; set; }
        public string? ImageUrl { get; set; }
        public int? Quantity { get; set; }
    }
}
