namespace VegaCityApp.API.Payload.Response.ETagResponse
{
    public class GetETagResponse : ResponseAPI
    {
        public Guid? EtagTypeId { get; set; }
        public Guid? MarketZoneId { get; set; }
        public string? Qrcode { get; set; }
    }
}
