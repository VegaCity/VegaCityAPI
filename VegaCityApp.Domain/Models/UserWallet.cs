using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class UserWallet
    {
        public UserWallet()
        {
            Transactions = new HashSet<Transaction>();
            UserActions = new HashSet<UserAction>();
        }

        public Guid Id { get; set; }
        public Guid? WalletTypeId { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }
        public int? Balance { get; set; }
        public int? BalanceHistory { get; set; }
        public bool? Deflag { get; set; }
        public Guid? UserId { get; set; }

        public virtual User? User { get; set; }
        public virtual WalletType? WalletType { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<UserAction> UserActions { get; set; }
    }
}
