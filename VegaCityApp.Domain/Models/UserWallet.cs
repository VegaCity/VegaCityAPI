using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class UserWallet
    {
        public UserWallet()
        {
            Deposits = new HashSet<Deposit>();
            Transactions = new HashSet<Transaction>();
        }

        public Guid Id { get; set; }
        public int? WalletType { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }
        public int? Balance { get; set; }
        public int? BalanceHistory { get; set; }
        public bool? Deflag { get; set; }
        public Guid? UserId { get; set; }
        public Guid? EtagId { get; set; }

        public virtual Etag? Etag { get; set; }
        public virtual User? User { get; set; }
        public virtual ICollection<Deposit> Deposits { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
