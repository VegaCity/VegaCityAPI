namespace VegaCityApp.API.Payload.Request.Promotion
{
    public class PromotionRequest
    {
        public string Name { get; set; } = null!;
        public string PromotionCode { get; set; } = null!;
        public string? Description { get; set; }
        public int? MaxDiscount { get; set; }
        public int? RequireAmount { get; set; }
        public int? Quantity { get; set; }
        public double? DiscountPercent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
