namespace VegaCityApp.API.Payload.Request
{
    public class UpdateStoreRequest
    {
        public Guid StoreId { get; set; }
        public string Name { get; set; }
        public int StoreStatus { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ShortName { get; set; }
        public string? Email { get; set; }
        public string? Description { get; set; }
    }
}
