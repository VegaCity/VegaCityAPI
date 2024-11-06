namespace VegaCityApp.API.Payload.Request.Package
{
    public class CreatePackageItemRequest
    {
        public Guid PackageId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
