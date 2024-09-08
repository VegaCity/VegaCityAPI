using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class DisputeReport
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string? IssueType { get; set; }
        public string? Description { get; set; }
        public string? Resolution { get; set; }
        public string? ResolvedBy { get; set; }
        public DateTime? CrDate { get; set; }
        public int? Status { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public Guid? StoreId { get; set; }

        public virtual Store? Store { get; set; }
        public virtual User? User { get; set; }
    }
}
