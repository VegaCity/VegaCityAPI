using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class ProductCategory
    {
        public ProductCategory()
        {
            Products = new HashSet<Product>();
            WalletTypeMappings = new HashSet<WalletTypeMapping>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public bool Deflag { get; set; }
        public string? Description { get; set; }

        public virtual ICollection<Product> Products { get; set; }
        public virtual ICollection<WalletTypeMapping> WalletTypeMappings { get; set; }
    }
}
