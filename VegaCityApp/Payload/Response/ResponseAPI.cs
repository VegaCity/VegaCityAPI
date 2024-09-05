using VegaCityApp.Domain.Models;

namespace VegaCityApp.API.Payload.Response
{
    public class ResponseAPI
    {
        public int StatusCode { get; set; }
        public string MessageResponse { get; set; }
        public Object Data { get; set; }
    }
    public class EtagTypeResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid MarketZoneId { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? BonusRate { get; set; }
        public bool Deflag { get; set; }
        public int Amount { get; set; }
    }
}
