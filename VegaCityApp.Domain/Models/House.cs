using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class House
    {
        public House()
        {
            Stores = new HashSet<Store>();
        }

        public Guid Id { get; set; }
        public string HouseName { get; set; } = null!;
        public string? Location { get; set; }
        public string? Address { get; set; }
        public Guid ZoneId { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public bool Deflag { get; set; }
        public bool Isrent { get; set; }

        public virtual Zone Zone { get; set; } = null!;
        public virtual ICollection<Store> Stores { get; set; }
    }
}
