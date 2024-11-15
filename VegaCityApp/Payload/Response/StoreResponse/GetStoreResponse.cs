using VegaCityApp.Domain.Models;

namespace VegaCityApp.API.Payload.Response.StoreResponse
{
    public class GetStoreResponse
    {
        public Guid Id { get; set; }
        public int? StoreType { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public bool Deflag { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string? ShortName { get; set; }
        public string Email { get; set; } = null!;
        public Guid? ZoneId { get; set; }
        public Guid MarketZoneId { get; set; }
        public string? Description { get; set; }
        public int Status { get; set; }
        public string? ZoneName { get; set; }
    }
}
