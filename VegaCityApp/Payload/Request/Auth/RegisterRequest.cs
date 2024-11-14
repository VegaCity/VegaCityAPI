namespace VegaCityApp.API.Payload.Request.Auth
{
    public class RegisterRequest
    {
        public Guid apiKey { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string CccdPassport { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string? Description { get; set; }
        public string RoleName { get; set; }
        public int? StoreType { get; set; }
    }
}
