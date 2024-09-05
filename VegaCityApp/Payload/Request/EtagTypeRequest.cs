namespace VegaCityApp.API.Payload.Request
{
    public class EtagTypeRequest
    {
        public string Name { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? BonusRate { get; set; }
    }
    public class UpdateEtagTypeRequest
    {
        public Guid EtagTypeId { get; set; }
        public string Name { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? BonusRate { get; set; }
    }
}
