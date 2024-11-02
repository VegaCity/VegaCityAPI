using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class CustomerMoneyTransfer
    {
        public Guid Id { get; set; }
        public Guid? MarketZoneId { get; set; }
        public Guid? PackageItemId { get; set; }
        public int? Amount { get; set; }
        public string? Status { get; set; }
        public bool? IsIncrease { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }

        public virtual MarketZone? MarketZone { get; set; }
        public virtual PackageItem? PackageItem { get; set; }
    }
}
