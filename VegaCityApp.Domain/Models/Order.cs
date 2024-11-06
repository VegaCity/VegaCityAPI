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
            PackageOrders = new HashSet<PackageOrder>();
            PromotionOrders = new HashSet<PromotionOrder>();
            Transactions = new HashSet<Transaction>();
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
        public Guid? PackageItemId { get; set; }
        public Guid? PackageId { get; set; }
        public Guid UserId { get; set; }
        public string SaleType { get; set; } = null!;

        public virtual Package? Package { get; set; }
        public virtual PackageItem? PackageItem { get; set; }
        public virtual Store? Store { get; set; }
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Deposit> Deposits { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<PackageOrder> PackageOrders { get; set; }
        public virtual ICollection<PromotionOrder> PromotionOrders { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
