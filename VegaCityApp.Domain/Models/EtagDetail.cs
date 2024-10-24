using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class EtagDetail
    {
        public Guid Id { get; set; }
        public Guid EtagId { get; set; }
        public string? FullName { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string CccdPassport { get; set; } = null!;
        public int? Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public bool IsVerifyPhone { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }

        public virtual Etag Etag { get; set; } = null!;
    }
}
