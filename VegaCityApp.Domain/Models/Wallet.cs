using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Wallet
    {
        public Wallet()
        {
            Transactions = new HashSet<Transaction>();
        }

        public Guid Id { get; set; }
        public string? Name { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public int Balance { get; set; }
        public int BalanceHistory { get; set; }
        public int? BalanceStart { get; set; }
        public Guid? UserId { get; set; }
        public Guid? StoreId { get; set; }
        public Guid WalletTypeId { get; set; }
        public Guid? PackageOrderId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool Deflag { get; set; }

        public virtual PackageOrder? PackageOrder { get; set; }
        public virtual Store? Store { get; set; }
        public virtual User? User { get; set; }
        public virtual WalletType WalletType { get; set; } = null!;
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
