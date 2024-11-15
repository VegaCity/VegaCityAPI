using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class OrderDetail
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid? ProductId { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public int FinalAmount { get; set; }
        public int? PromotionAmount { get; set; }
        public int Vatamount { get; set; }
        public int Quantity { get; set; }
        public int Amount { get; set; }

        public virtual Order Order { get; set; } = null!;
        public virtual Product? Product { get; set; }
    }
}
