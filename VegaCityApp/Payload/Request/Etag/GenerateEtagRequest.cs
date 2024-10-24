namespace VegaCityApp.API.Payload.Request.Etag
{
    public class GenerateEtagRequest
    {
        public Guid? EtagId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
