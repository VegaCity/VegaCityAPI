using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class MarketZoneCard
    {
        public MarketZoneCard()
        {
            Transactions = new HashSet<Transaction>();
            UserActions = new HashSet<UserAction>();
        }

        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public Guid? MarketZoneId { get; set; }
        public int? Balance { get; set; }
        public string? Cccd { get; set; }
        public string? ImageUrl { get; set; }
        public int? Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Qrcode { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }
        public bool? Deflag { get; set; }
        public Guid? MarketZoneCardTypeId { get; set; }

        public virtual MarketZone? MarketZone { get; set; }
        public virtual MarketZoneCardType? MarketZoneCardType { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<UserAction> UserActions { get; set; }
    }
}
