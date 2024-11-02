using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class PromotionOrder
    {
        public Guid Id { get; set; }
        public Guid PromotionId { get; set; }
        public Guid OrderId { get; set; }
        public int DiscountAmount { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public bool Deflag { get; set; }

        public virtual Order Order { get; set; } = null!;
        public virtual Promotion Promotion { get; set; } = null!;
    }
}
