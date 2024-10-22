namespace VegaCityApp.API.Payload.Request.Admin
{
    public class UpdateUserAccountRequest
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? Birthday { get; set; }
        public int? Gender { get; set; }
        public string? Cccd { get; set; }
        public string? ImageUrl { get; set; }
    }
}
