namespace VegaCityApp.API.Payload.Request.Package
{
    public class CreatePackageItemRequest
    {
        public Guid PackageId { get; set; }
        public string CusName { get; set; } = null!;
        public string CusEmail { get; set; } = null!;
        public string CusCccdpassport { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
    }
}
