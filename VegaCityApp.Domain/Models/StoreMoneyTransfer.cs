using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class StoreMoneyTransfer
    {
        public Guid Id { get; set; }
        public Guid StoreId { get; set; }
        public Guid MarketZoneId { get; set; }
        public int Amount { get; set; }
        public bool IsIncrease { get; set; }
        public string? Description { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public string Status { get; set; } = null!;
        public Guid TransactionId { get; set; }

        public virtual MarketZone MarketZone { get; set; } = null!;
        public virtual Store Store { get; set; } = null!;
        public virtual Transaction Transaction { get; set; } = null!;
    }
}
