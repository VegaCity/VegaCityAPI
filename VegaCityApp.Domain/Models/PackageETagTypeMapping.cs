using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class PackageETagTypeMapping
    {
        public Guid Id { get; set; }
        public Guid PackageId { get; set; }
        public Guid EtagTypeId { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }

        public virtual EtagType EtagType { get; set; } = null!;
        public virtual Package Package { get; set; } = null!;
    }
}
