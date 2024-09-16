using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Order
    {
        public Order()
        {
            Deposits = new HashSet<Deposit>();
        }

        public Guid Id { get; set; }
        public string PaymentType { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int TotalAmount { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public string Status { get; set; } = null!;
        public string InvoiceId { get; set; } = null!;
        public double Vatrate { get; set; }
        public string ProductJson { get; set; } = null!;
        public Guid StoreId { get; set; }
        public Guid? EtagId { get; set; }

        public virtual Etag? Etag { get; set; }
        public virtual Store Store { get; set; } = null!;
        public virtual ICollection<Deposit> Deposits { get; set; }
    }
}
