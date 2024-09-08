namespace VegaCityApp.API.Payload.Request
{
    public class EtagTypeRequest
    {
        public string Name { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? BonusRate { get; set; }
        public int Amount { get; set; }
    }
    public class UpdateEtagTypeRequest
    {
        public string Name { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? BonusRate { get; set; }
        public int? Amount { get; set; }
    }
}
