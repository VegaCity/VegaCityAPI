namespace VegaCityApp.API.Payload.Request.Etag
{
    public class UpdateEtagRequest
    {
        public string? Fullname { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ImageUrl { get; set; }
        public int? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
      //  public string? CCCD { get; set; }
    }
}
