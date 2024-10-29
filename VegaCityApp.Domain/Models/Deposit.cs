using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Deposit
    {
        public Guid Id { get; set; }
        public string PaymentType { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool IsIncrease { get; set; }
        public int Amount { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public Guid WalletId { get; set; }
        public Guid EtagId { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? StoreId { get; set; }

        public virtual Etag Etag { get; set; } = null!;
        public virtual Order? Order { get; set; }
        public virtual Store? Store { get; set; }
        public virtual Wallet Wallet { get; set; } = null!;
    }
}
