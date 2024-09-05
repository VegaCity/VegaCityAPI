namespace VegaCityApp.API.Payload.Request
{
    public class UpddatePackageRequest
    {
        public Guid? PackageId { get; set; }
        public string? Name { get; set; }
        public int? Price { get; set; }

        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? MarketZoneId { get; set; }
        public Guid? ETagTypeId { get; set; }
    }
}
