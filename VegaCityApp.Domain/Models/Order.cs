using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Order
    {
        public Order()
        {
            OrderDetails = new HashSet<OrderDetail>();
            Transactions = new HashSet<Transaction>();
            UserActions = new HashSet<UserAction>();
        }

        public Guid Id { get; set; }
        public string? Name { get; set; }
        public int? TotalAmount { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }
        public string? Status { get; set; }
        public Guid? StoreId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? MarketZoneCardId { get; set; }
        public string? PaymentType { get; set; }
        public string? InvoiceId { get; set; }
        public double? Vatrate { get; set; }
        public Guid? StoreSessionId { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<UserAction> UserActions { get; set; }
    }
}
