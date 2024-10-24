namespace VegaCityApp.API.Payload.Request.Etag
{
    public class ActivateEtagRequest
    {
        public string? CCCD { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public int Gender { get; set; }
        public DateTime Birthday { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
