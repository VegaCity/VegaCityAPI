namespace VegaCityApp.API.Payload.Response.WalletResponse
{
    public class WalletTypeResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid MarketZoneId { get; set; }
        public DateTime crDate { get; set; }
        public DateTime upsDate { get; set; }
        public bool Deflag { get; set; }
    }
}
