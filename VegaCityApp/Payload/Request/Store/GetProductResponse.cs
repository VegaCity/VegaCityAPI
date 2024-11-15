namespace VegaCityApp.API.Payload.Request.Store
{
    public class GetProductResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid ProductCategoryId { get; set; }
        public Guid MenuId { get; set; }
        public int Price { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public string Status { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string ProductCategoryName { get; set; } = null!;
    }
}
