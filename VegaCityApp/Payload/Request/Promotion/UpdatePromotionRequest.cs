namespace VegaCityApp.API.Payload.Request.Promotion
{
    public class UpdatePromotionRequest
    {
        public string Name { get; set; }
        public String? Description { get; set; }
        public int? MaxDiscount { get; set; }
        public int? Quantity { get; set; }
        public float? DiscountPercent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
