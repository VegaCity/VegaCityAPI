namespace VegaCityApp.API.Payload.Response.StoreResponse
{
    public class ServiceStoreResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid StoreId { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public bool Deflag { get; set; }
    }
}
