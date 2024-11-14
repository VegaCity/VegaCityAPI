namespace VegaCityApp.API.Payload.Request.Store
{
    public class ServiceStoreRequest
    {
        public string Name { get; set; }
        public Guid StoreId { get; set; }
        public int Price { get; set; }
    }
}
