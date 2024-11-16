using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class MenuProductMapping
    {
        public Guid Id { get; set; }
        public Guid MenuId { get; set; }
        public Guid ProductId { get; set; }
        public DateTime CrDate { get; set; }

        public virtual Menu Menu { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
