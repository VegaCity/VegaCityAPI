namespace VegaCityApp.API.Payload.Request.Package
{
    public class UpdatePackageRequest
    {
        public string? ImageUrl { get; set; }
        public string? Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? Price { get; set; }
        public int? Duration { get; set; }
    }
}
