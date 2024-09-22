using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Menu
    {
        public Menu()
        {
            OrderDetails = new HashSet<OrderDetail>();
            ProductCategories = new HashSet<ProductCategory>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CrDate { get; set; }
        public bool Deflag { get; set; }
        public Guid StoreId { get; set; }
        public string? ImageUrl { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? MenuJson { get; set; }

        public virtual Store Store { get; set; } = null!;
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<ProductCategory> ProductCategories { get; set; }
    }
}
