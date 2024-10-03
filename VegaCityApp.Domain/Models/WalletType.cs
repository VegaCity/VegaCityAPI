using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class WalletType
    {
        public WalletType()
        {
            EtagTypes = new HashSet<EtagType>();
            WalletTypeStoreServiceMappings = new HashSet<WalletTypeStoreServiceMapping>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid MarketZoneId { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public bool Deflag { get; set; }

        public virtual MarketZone MarketZone { get; set; } = null!;
        public virtual ICollection<EtagType> EtagTypes { get; set; }
        public virtual ICollection<WalletTypeStoreServiceMapping> WalletTypeStoreServiceMappings { get; set; }
    }
}
