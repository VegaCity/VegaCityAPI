namespace VegaCityApp.API.Payload.Request.Etag
{
    public class GenerateEtagRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Day { get; set; }
        public int? MoneyStart { get; set; }
    }
}
