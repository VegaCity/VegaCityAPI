using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class DisputeReport
    {
        public Guid Id { get; set; }
        public Guid IssueTypeId { get; set; }
        public string Creator { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Solution { get; set; }
        public string? SolveBy { get; set; }
        public DateTime CrDate { get; set; }
        public int Status { get; set; }
        public DateTime? SolveDate { get; set; }
        public Guid? StoreId { get; set; }

        public virtual IssueType IssueType { get; set; } = null!;
        public virtual Store? Store { get; set; }
    }
}
