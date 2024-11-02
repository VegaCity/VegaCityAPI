using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Deposit
    {
        public Guid Id { get; set; }
        public string? PaymentType { get; set; }
        public string? Name { get; set; }
        public bool? IsIncrease { get; set; }
        public int? Amount { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }
        public Guid? PackageItemId { get; set; }
        public Guid? WalletId { get; set; }
        public Guid? OrderId { get; set; }

        public virtual Order? Order { get; set; }
        public virtual PackageItem? PackageItem { get; set; }
        public virtual WalletType? Wallet { get; set; }
    }
}
