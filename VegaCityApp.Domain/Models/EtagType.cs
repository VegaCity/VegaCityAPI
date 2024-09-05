﻿using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class EtagType
    {
        public EtagType()
        {
            Etags = new HashSet<Etag>();
            PackageETagTypeMappings = new HashSet<PackageETagTypeMapping>();
        }

        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid? MarketZoneId { get; set; }
        public string? ImageUrl { get; set; }

        public virtual MarketZone? MarketZone { get; set; }
        public virtual ICollection<Etag> Etags { get; set; }
        public virtual ICollection<PackageETagTypeMapping> PackageETagTypeMappings { get; set; }
    }
}
