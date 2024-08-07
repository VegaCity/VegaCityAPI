using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class MarketZone
    {
        public MarketZone()
        {
            MarketZoneCardTypes = new HashSet<MarketZoneCardType>();
            MarketZoneCards = new HashSet<MarketZoneCard>();
            Stores = new HashSet<Store>();
            Transactions = new HashSet<Transaction>();
            UserActionTypes = new HashSet<UserActionType>();
            WalletTypes = new HashSet<WalletType>();
        }

        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Location { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? ShortName { get; set; }
        public bool? Deflag { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }

        public virtual ICollection<MarketZoneCardType> MarketZoneCardTypes { get; set; }
        public virtual ICollection<MarketZoneCard> MarketZoneCards { get; set; }
        public virtual ICollection<Store> Stores { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<UserActionType> UserActionTypes { get; set; }
        public virtual ICollection<WalletType> WalletTypes { get; set; }
    }
}
