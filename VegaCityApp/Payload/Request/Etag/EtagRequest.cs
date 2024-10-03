namespace VegaCityApp.API.Payload.Request.Etag
{
    public class EtagRequest
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Cccd { get; set; }
        public int Gender { get; set; }
        public Guid EtagTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
