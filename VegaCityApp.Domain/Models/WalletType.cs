using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class WalletType
    {
        public WalletType()
        {
            UserWallets = new HashSet<UserWallet>();
        }

        public Guid Id { get; set; }
        public string? Name { get; set; }
        public decimal? BonusRate { get; set; }
        public Guid? MarketZoneId { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }
        public bool? Deflag { get; set; }

        public virtual MarketZone? MarketZone { get; set; }
        public virtual ICollection<UserWallet> UserWallets { get; set; }
    }
}
