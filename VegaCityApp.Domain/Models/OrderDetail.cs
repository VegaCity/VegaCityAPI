using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class OrderDetail
    {
        public Guid Id { get; set; }
        public Guid? OrderId { get; set; }
        public string? ProductJson { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }
        public double? TotalAmount { get; set; }
        public int? Quantity { get; set; }
        public double? Vatrate { get; set; }

        public virtual Order? Order { get; set; }
    }
}
