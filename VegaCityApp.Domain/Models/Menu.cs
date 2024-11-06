using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Menu
    {
        public Menu()
        {
            Products = new HashSet<Product>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public bool Deflag { get; set; }
        public Guid StoreId { get; set; }
        public string? ImageUrl { get; set; }
        public string Address { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string MenuJson { get; set; } = null!;

        public virtual Store Store { get; set; } = null!;
        public virtual ICollection<Product> Products { get; set; }
    }
}
