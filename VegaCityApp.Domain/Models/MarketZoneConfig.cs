using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class MarketZoneConfig
    {
        public Guid Id { get; set; }
        public Guid MarketZoneId { get; set; }
        public double StoreStranferRate { get; set; }
        public double WithdrawExpireRate { get; set; }

        public virtual MarketZone MarketZone { get; set; } = null!;
    }
}
