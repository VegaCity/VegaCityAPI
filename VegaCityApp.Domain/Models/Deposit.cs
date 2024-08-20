using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Deposit
    {
        public Deposit()
        {
            Transactions = new HashSet<Transaction>();
        }

        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? IsIncrease { get; set; }
        public int? Amount { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }
        public Guid? UserWalletId { get; set; }

        public virtual UserWallet? UserWallet { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
