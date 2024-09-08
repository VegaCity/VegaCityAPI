namespace VegaCityApp.API.Payload.Request.Auth
{
    public class RegisterRequest
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string CCCD { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string? Description { get; set; }
    }
}
