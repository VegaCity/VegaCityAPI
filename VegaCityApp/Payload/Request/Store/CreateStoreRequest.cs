namespace VegaCityApp.API.Payload.Request.Store
{
    public class CreateStoreRequest
    {
        public int StoreType { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? ShortName { get; set; }
        public string Email { get; set; } = null!;
        public string? Description { get; set; }
        public int Status { get; set; }
    }
}
