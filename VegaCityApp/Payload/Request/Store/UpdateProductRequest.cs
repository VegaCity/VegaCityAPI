namespace VegaCityApp.API.Payload.Request.Store
{
    public class UpdateProductRequest
    {
        public string? Name { get; set; }
        public int? Price { get; set; }
        public string? ImageUrl { get; set; }
        public int? Quantity { get; set; }
        public int? Duration { get; set; }
        public string? Unit { get; set; }
    }
}
