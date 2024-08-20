using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Zone
    {
        public Zone()
        {
            Houses = new HashSet<House>();
        }

        public Guid Id { get; set; }
        public Guid MarketZoneId { get; set; }
        public string? Name { get; set; }
        public string? Location { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }

        public virtual MarketZone MarketZone { get; set; } = null!;
        public virtual ICollection<House> Houses { get; set; }
    }
}
