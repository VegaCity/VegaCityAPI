using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Transaction
    {
        public Transaction()
        {
            CustomerMoneyTransfers = new HashSet<CustomerMoneyTransfer>();
            StoreMoneyTransfers = new HashSet<StoreMoneyTransfer>();
        }

        public Guid Id { get; set; }
        public string? Type { get; set; }
        public Guid? WalletId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? DespositId { get; set; }
        public Guid? StoreId { get; set; }
        public string Status { get; set; } = null!;
        public bool IsIncrease { get; set; }
        public string? Description { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public int Amount { get; set; }
        public string Currency { get; set; } = null!;
        public Guid? OrderId { get; set; }

        public virtual Order? Order { get; set; }
        public virtual Store? Store { get; set; }
        public virtual User? User { get; set; }
        public virtual Wallet? Wallet { get; set; }
        public virtual ICollection<CustomerMoneyTransfer> CustomerMoneyTransfers { get; set; }
        public virtual ICollection<StoreMoneyTransfer> StoreMoneyTransfers { get; set; }
    }
}
