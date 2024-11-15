using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Payment
    {
        public Payment()
        {
            Transactions = new HashSet<Transaction>();
        }

        public Guid Id { get; set; }
        public string? Name { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }
        public string? Status { get; set; }
        public int? FinalAmount { get; set; }
        public Guid? OrderId { get; set; }

        public virtual Order? Order { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
