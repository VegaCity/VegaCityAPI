using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Promotion
    {
        public Promotion()
        {
            PromotionOrders = new HashSet<PromotionOrder>();
        }

        public Guid Id { get; set; }
        public Guid MarketZoneId { get; set; }
        public string Name { get; set; } = null!;
        public string PromotionCode { get; set; } = null!;
        public string? Description { get; set; }
        public int? MaxDiscount { get; set; }
        public int? Quantity { get; set; }
        public double? DiscountPercent { get; set; }
        public int Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public virtual MarketZone MarketZone { get; set; } = null!;
        public virtual ICollection<PromotionOrder> PromotionOrders { get; set; }
    }
}
