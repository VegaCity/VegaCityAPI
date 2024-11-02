namespace VegaCityApp.API.Payload.Request.Package
{
    public class CreatePackageTypeRequest
    {
        public Guid MarketZoneId { get; set; }
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        //public bool Deflag { get; set; } 

    }
}
