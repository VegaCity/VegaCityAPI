using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class MarketZone
    {
        public MarketZone()
        {
            CustomerMoneyTransfers = new HashSet<CustomerMoneyTransfer>();
            PackageTypes = new HashSet<PackageType>();
            Promotions = new HashSet<Promotion>();
            StoreMoneyTransfers = new HashSet<StoreMoneyTransfer>();
            Users = new HashSet<User>();
            WalletTypes = new HashSet<WalletType>();
            Zones = new HashSet<Zone>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public string Address { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? ShortName { get; set; }
        public bool Deflag { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }

        public virtual MarketZoneConfig? MarketZoneConfig { get; set; }
        public virtual ICollection<CustomerMoneyTransfer> CustomerMoneyTransfers { get; set; }
        public virtual ICollection<PackageType> PackageTypes { get; set; }
        public virtual ICollection<Promotion> Promotions { get; set; }
        public virtual ICollection<StoreMoneyTransfer> StoreMoneyTransfers { get; set; }
        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<WalletType> WalletTypes { get; set; }
        public virtual ICollection<Zone> Zones { get; set; }
    }
}
