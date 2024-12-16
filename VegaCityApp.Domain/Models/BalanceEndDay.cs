using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class BalanceEndDay
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public Guid? StoreId { get; set; }
        public DateTime? DateCheck { get; set; }
        public int? Balance { get; set; }
        public int? BalanceHistory { get; set; }
        public bool? Deflag { get; set; }

        public virtual Store? Store { get; set; }
        public virtual User? User { get; set; }
    }
}
