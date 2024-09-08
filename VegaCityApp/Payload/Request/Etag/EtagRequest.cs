namespace VegaCityApp.API.Payload.Request.Etag
{
    public class EtagRequest
    {
        public Guid? UserId { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public int? Balance { get; set; }
        public string? Cccd { get; set; }
        public string? ImageUrl { get; set; }
        public int? Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public Guid? EtagTypeId { get; set; }
    }
}
