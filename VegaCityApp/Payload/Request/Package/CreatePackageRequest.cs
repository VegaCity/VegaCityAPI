namespace VegaCityApp.API.Payload.Request.Package
{
    public class CreatePackageRequest
    {
        public string Type { get; set; } = null!;
        public Guid ZoneId { get; set; }
        public string? ImageUrl { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int Price { get; set; }
        public int Duration { get; set; }
        public int MoneyStart { get; set; }
        public Guid WalletTypeId { get; set; }
    }
}
