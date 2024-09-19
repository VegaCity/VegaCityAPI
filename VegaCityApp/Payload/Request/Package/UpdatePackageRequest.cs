namespace VegaCityApp.API.Payload.Request.Package
{
    public class UpdatePackageRequest
    {
        public string? Name { get; set; }
        public int? Price { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
