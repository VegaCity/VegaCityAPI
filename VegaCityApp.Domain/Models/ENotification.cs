using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class ENotification
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid? EtagId { get; set; }
        public DateTime? CrDate { get; set; }
        public int? Status { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? ExpireDate { get; set; }
        public int? Type { get; set; }

        public virtual Etag? Etag { get; set; }
    }
}
