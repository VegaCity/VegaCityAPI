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
        public string? PaymentType { get; set; }
        public string? Name { get; set; }
        public int? TotalAmount { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }
        public string? Status { get; set; }
        public string? InvoiceId { get; set; }
        public double? Vatrate { get; set; }
        public string? ProductJson { get; set; }
        public Guid? StoreId { get; set; }
        public Guid? EtagId { get; set; }

        public virtual Etag? Etag { get; set; }
        public virtual Store? Store { get; set; }
        public virtual ICollection<Deposit> Deposits { get; set; }
    }
}
