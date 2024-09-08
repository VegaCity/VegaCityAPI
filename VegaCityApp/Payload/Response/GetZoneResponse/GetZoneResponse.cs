namespace VegaCityApp.API.Payload.Response.GetZoneResponse
{
    public class GetZoneResponse
    {
        public Guid MarketZoneId { get; set; }
        public string Name { get; set; } = null!;
        public string? Location { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public bool Deflag { get; set; }
    }
}
