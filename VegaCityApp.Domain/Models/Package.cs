using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Package
    {
        public Package()
        {
            PackageETagTypeMappings = new HashSet<PackageETagTypeMapping>();
        }

        public Guid Id { get; set; }
        public string? ImageUrl { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? Price { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public bool Deflag { get; set; }

        public virtual ICollection<PackageETagTypeMapping> PackageETagTypeMappings { get; set; }
    }
}
