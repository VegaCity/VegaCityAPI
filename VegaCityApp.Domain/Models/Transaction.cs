using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Transaction
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = null!;
        public Guid? WalletId { get; set; }
        public Guid? StoreId { get; set; }
        public string Status { get; set; } = null!;
        public bool IsIncrease { get; set; }
        public string? Description { get; set; }
        public DateTime CrDate { get; set; }
        public int Amount { get; set; }
        public string Currency { get; set; } = null!;

        public virtual Store? Store { get; set; }
        public virtual Wallet? Wallet { get; set; }
    }
}
