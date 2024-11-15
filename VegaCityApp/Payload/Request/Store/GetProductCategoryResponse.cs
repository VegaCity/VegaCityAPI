namespace VegaCityApp.API.Payload.Request.Store
{
    public class GetProductCategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public bool Deflag { get; set; }
        public string? Description { get; set; }
    }
}
