using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Wallet
    {
        public Wallet()
        {
            Deposits = new HashSet<Deposit>();
            PackageItems = new HashSet<PackageItem>();
            Transactions = new HashSet<Transaction>();
        }

        public Guid Id { get; set; }
        public Guid WalletTypeId { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public int Balance { get; set; }
        public int BalanceHistory { get; set; }
        public bool Deflag { get; set; }
        public Guid? UserId { get; set; }
        public Guid? StoreId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public virtual Store? Store { get; set; }
        public virtual User? User { get; set; }
        public virtual WalletType WalletType { get; set; } = null!;
        public virtual ICollection<Deposit> Deposits { get; set; }
        public virtual ICollection<PackageItem> PackageItems { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
