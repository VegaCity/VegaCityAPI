namespace VegaCityApp.API.Payload.Request.Store
{
    public class GetMenuResponse
    {
        public Guid Id { get; set; }
        public Guid StoreId { get; set; }
        public string? ImageUrl { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public bool Deflag { get; set; }
        public int DateFilter { get; set; }
    }
}
