using VegaCityApp.API.Enums;

namespace VegaCityApp.API.Payload.Request.Store
{
    public class UpdateStoreRequest
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ShortName { get; set; }
        public string? Email { get; set; }
        public string? Description { get; set; }
        public int? Status { get; set; }
        public int? StoreType { get; set; }
    }
}
