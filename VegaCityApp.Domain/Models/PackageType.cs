using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class PackageType
    {
        public PackageType()
        {
            Packages = new HashSet<Package>();
        }

        public Guid Id { get; set; }
        public Guid? MarketZoneId { get; set; }
        public string? Name { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }
        public bool? Deflag { get; set; }

        public virtual MarketZone? MarketZone { get; set; }
        public virtual ICollection<Package> Packages { get; set; }
    }
}
