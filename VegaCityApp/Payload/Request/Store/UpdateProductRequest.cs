namespace VegaCityApp.API.Payload.Request.Store
{
    public class UpdateProductRequest
    {
        public string? Name { get; set; }
        public int? Price { get; set; }
        public string? Status { get; set; }
        public string? ImageUrl { get; set; }
    }
}
