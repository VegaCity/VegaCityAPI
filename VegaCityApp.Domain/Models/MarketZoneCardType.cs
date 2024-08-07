using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class MarketZoneCardType
    {
        public MarketZoneCardType()
        {
            MarketZoneCards = new HashSet<MarketZoneCard>();
        }

        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid? MarketZoneId { get; set; }
        public string? ImageUrl { get; set; }

        public virtual MarketZone? MarketZone { get; set; }
        public virtual ICollection<MarketZoneCard> MarketZoneCards { get; set; }
    }
}
