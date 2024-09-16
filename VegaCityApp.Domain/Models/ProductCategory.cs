using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class ProductCategory
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid StoreId { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public string ProductJson { get; set; } = null!;

        public virtual Store Store { get; set; } = null!;
    }
}
