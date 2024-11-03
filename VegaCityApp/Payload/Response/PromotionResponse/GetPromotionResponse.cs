using Newtonsoft.Json;
using VegaCityApp.Domain.Models;

namespace VegaCityApp.API.Payload.Response.PromotionResponse
{
    public class GetPromotionResponse : ResponseAPI
    {
        public Guid Id { get; set; }
        public Guid MarketZoneId { get; set; }
        public string Name { get; set; }
        public string PromotionCode { get; set; }
        public String? Description { get; set; }    
        public int? MaxDiscount { get; set; }
        public int? Quantity { get; set; }
        public float? DiscountPercent { get; set; }
        public int Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [JsonIgnore]
        public virtual MarketZone? MarketZone { get; set; }
    }

    public class GetListPromotionResponse : ResponseAPI
    {
        public Guid Id { get; set; }
        public Guid MarketZoneId { get; set; }
        public string Name { get; set; }
        public string PromotionCode { get; set; }
        public String? Description { get; set; }
        public int? MaxDiscount { get; set; }
        public int? Quantity { get; set; }
        public double? DiscountPercent { get; set; }
        public int Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public virtual MarketZone? MarketZone { get; set; }
    }

}
