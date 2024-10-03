namespace VegaCityApp.API.Payload.Request.Etag
{
    public class GenerateEtagRequest
    {
        public Guid WalletTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Day { get; set; }
    }
}
