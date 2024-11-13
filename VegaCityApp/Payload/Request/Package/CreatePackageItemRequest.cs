namespace VegaCityApp.API.Payload.Request.Package
{
    public class CreatePackageItemRequest
    {
        public Guid PackageId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? PackageItemId { get; set; }//khi co, day se la package da dc activate cua parent, etahg chillds se generate dua tren cai nay

    }
}
