using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class MarketZone
    {
        public MarketZone()
        {
            Etags = new HashSet<Etag>();
            Packages = new HashSet<Package>();
            Stores = new HashSet<Store>();
            Zones = new HashSet<Zone>();
        }

        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Location { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? ShortName { get; set; }
        public bool? Deflag { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }

        public virtual ICollection<Etag> Etags { get; set; }
        public virtual ICollection<Package> Packages { get; set; }
        public virtual ICollection<Store> Stores { get; set; }
        public virtual ICollection<Zone> Zones { get; set; }
    }
}
