namespace VegaCityApp.API.Payload.Request.Etag
{
    public class EtagRequest
    {
        public string FullName { get; set; } //no
        public string PhoneNumber { get; set; } //no
        public string CccdPassport { get; set; } //no
        public int Gender { get; set; }
        public Guid EtagTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsAdult { get; set; }
    }
}
