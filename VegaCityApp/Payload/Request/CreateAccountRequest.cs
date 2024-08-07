namespace VegaCityApp.API.Payload.Request
{
    public class CreateAccountRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime Birthday { get; set; }
        public int? Gender { get; set; }
        public string? Cccd { get; set; }
        public string? ImageUrl { get; set; }
    }
}
