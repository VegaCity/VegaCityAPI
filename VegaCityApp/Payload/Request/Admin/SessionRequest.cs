namespace VegaCityApp.API.Payload.Request.Admin
{
    public class SessionRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid ZoneId { get; set; }
    }
}
