using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class OrderDetail
    {
        public Guid Id { get; set; }
        public Guid? OrderId { get; set; }
        public string? Description { get; set; }
        public int? TotalAmount { get; set; }
        public Guid? MenuId { get; set; }
        public int? Vatamount { get; set; }
        public string? InvoiceId { get; set; }

        public virtual Menu? Menu { get; set; }
        public virtual Order? Order { get; set; }
    }
}
