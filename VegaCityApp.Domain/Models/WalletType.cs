using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class WalletType
    {
        public WalletType()
        {
            StoreServices = new HashSet<StoreService>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid MarketZoneId { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public bool Deflag { get; set; }

        public virtual MarketZone MarketZone { get; set; } = null!;
        public virtual ICollection<StoreService> StoreServices { get; set; }
    }
}
