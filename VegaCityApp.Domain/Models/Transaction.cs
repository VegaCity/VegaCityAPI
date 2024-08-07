using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Transaction
    {
        public Guid Id { get; set; }
        public Guid? UserWalletId { get; set; }
        public Guid? StoreId { get; set; }
        public Guid? MarketZoneCardId { get; set; }
        public Guid? OrderId { get; set; }
        public string? Status { get; set; }
        public string? PaymentType { get; set; }
        public bool? IsIncrease { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
        public Guid? MarketZoneId { get; set; }
        public DateTime? CrDate { get; set; }
        public string? Currency { get; set; }

        public virtual MarketZone? MarketZone { get; set; }
        public virtual MarketZoneCard? MarketZoneCard { get; set; }
        public virtual Order? Order { get; set; }
        public virtual Store? Store { get; set; }
        public virtual UserWallet? UserWallet { get; set; }
    }
}
