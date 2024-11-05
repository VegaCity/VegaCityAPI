namespace VegaCityApp.API.Payload.Request.Promotion
{
    public class PromotionRequest
    {
        public Guid MarketZoneId { get; set; }
        public string Name { get; set; }
        public string PromotionCode { get; set; }
        public String? Description { get; set; }
        public int? MaxDiscount { get; set; }
        public int? RequireAmount { get; set; }
        public int? Quantity { get; set; }
        public float? DiscountPercent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
