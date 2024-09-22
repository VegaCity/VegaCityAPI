namespace VegaCityApp.API.Payload.Request.Etag
{
    public class EtagRequest
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Cccd { get; set; }
        public Guid EtagTypeId { get; set; }
    }
}
