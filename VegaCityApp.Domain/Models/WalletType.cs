using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class WalletType
    {
        public WalletType()
        {
            PackageDetails = new HashSet<PackageDetail>();
            WalletTypeMappings = new HashSet<WalletTypeMapping>();
            WalletTypeStoreServiceMappings = new HashSet<WalletTypeStoreServiceMapping>();
            Wallets = new HashSet<Wallet>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid MarketZoneId { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public bool Deflag { get; set; }

        public virtual MarketZone MarketZone { get; set; } = null!;
        public virtual ICollection<PackageDetail> PackageDetails { get; set; }
        public virtual ICollection<WalletTypeMapping> WalletTypeMappings { get; set; }
        public virtual ICollection<WalletTypeStoreServiceMapping> WalletTypeStoreServiceMappings { get; set; }
        public virtual ICollection<Wallet> Wallets { get; set; }
    }
}
