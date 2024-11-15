using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Menu
    {
        public Menu()
        {
            MenuProductMappings = new HashSet<MenuProductMapping>();
        }

        public Guid Id { get; set; }
        public Guid StoreId { get; set; }
        public string? ImageUrl { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public bool Deflag { get; set; }
        public int DateFilter { get; set; }

        public virtual Store Store { get; set; } = null!;
        public virtual ICollection<MenuProductMapping> MenuProductMappings { get; set; }
    }
}
