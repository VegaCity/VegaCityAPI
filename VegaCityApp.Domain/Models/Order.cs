using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Order
    {
        public Order()
        {
            OrderDetails = new HashSet<OrderDetail>();
            Payments = new HashSet<Payment>();
            PromotionOrders = new HashSet<PromotionOrder>();
            Transactions = new HashSet<Transaction>();
        }

        public Guid Id { get; set; }
        public string SaleType { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int TotalAmount { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public string Status { get; set; } = null!;
        public string InvoiceId { get; set; } = null!;
        public Guid? StoreId { get; set; }
        public Guid? PackageOrderId { get; set; }
        public Guid? PackageId { get; set; }
        public Guid UserId { get; set; }

        public virtual Package? Package { get; set; }
        public virtual PackageOrder? PackageOrder { get; set; }
        public virtual Store? Store { get; set; }
        public virtual User User { get; set; } = null!;
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<PromotionOrder> PromotionOrders { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
