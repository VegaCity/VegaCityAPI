using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Order
    {
        public Order()
        {
            Deposits = new HashSet<Deposit>();
            OrderDetails = new HashSet<OrderDetail>();
        }

        public Guid Id { get; set; }
        public string PaymentType { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int TotalAmount { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public string Status { get; set; } = null!;
        public string InvoiceId { get; set; } = null!;
        public Guid? StoreId { get; set; }
        public Guid? EtagId { get; set; }
        public string? CustomerInfo { get; set; }
        public string? SaleType { get; set; }
        public Guid? UserId { get; set; }

        public virtual Etag? Etag { get; set; }
        public virtual Store? Store { get; set; }
        public virtual User? User { get; set; }
        public virtual ICollection<Deposit> Deposits { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
