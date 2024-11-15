namespace VegaCityApp.API.Payload.Request.Store
{
    public class CreateProductCategoryRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
