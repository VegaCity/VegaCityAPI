using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class CustomerMoneyTransfer
    {
        public Guid Id { get; set; }
        public Guid MarketZoneId { get; set; }
        public Guid PackageOrderId { get; set; }
        public Guid TransactionId { get; set; }
        public int Amount { get; set; }
        public string Status { get; set; } = null!;
        public bool IsIncrease { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }

        public virtual MarketZone MarketZone { get; set; } = null!;
        public virtual PackageOrder PackageOrder { get; set; } = null!;
        public virtual Transaction Transaction { get; set; } = null!;
    }
}
