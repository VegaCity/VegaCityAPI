﻿using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Deposit
    {
        public Guid Id { get; set; }
        public string? PaymentType { get; set; }
        public string? Name { get; set; }
        public string? IsIncrease { get; set; }
        public int? Amount { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }
        public Guid? WalletId { get; set; }
        public Guid? EtagId { get; set; }
        public Guid? OrderId { get; set; }

        public virtual Etag? Etag { get; set; }
        public virtual Order? Order { get; set; }
        public virtual Wallet? Wallet { get; set; }
    }
}
