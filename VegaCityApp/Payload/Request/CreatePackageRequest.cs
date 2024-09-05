namespace VegaCityApp.API.Payload.Request
{
    public class CreatePackageRequest
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public int Price { get; set; }
        public Guid? MarketZoneId { get; set; }

        public Guid? ETagTypeId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

    }
}
