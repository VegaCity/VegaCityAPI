namespace VegaCityApp.API.Payload.Request.Admin
{
    public class GetMarketZoneResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public string Address { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? ShortName { get; set; }
        public bool Deflag { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
    }
}
