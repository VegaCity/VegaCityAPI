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
        public string Name { get; set; } = null!;
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public string Status { get; set; } = null!;
        public int FinalAmount { get; set; }
        public Guid OrderId { get; set; }

        public virtual Order Order { get; set; } = null!;
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
