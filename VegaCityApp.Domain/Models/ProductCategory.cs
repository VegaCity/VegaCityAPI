using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class ProductCategory
    {
        public ProductCategory()
        {
            Products = new HashSet<Product>();
        }

        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid? MenuId { get; set; }
        public DateTime? CrDate { get; set; }

        public virtual Menu? Menu { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
