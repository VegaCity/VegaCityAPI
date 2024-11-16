using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Report
    {
        public Guid Id { get; set; }
        public string? PackageItemId { get; set; }
        public Guid? UserId { get; set; }
        public Guid IssueTypeId { get; set; }
        public string Description { get; set; } = null!;
        public string? Solution { get; set; }
        public string? SolveBy { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public int Status { get; set; }

        public virtual IssueType IssueType { get; set; } = null!;
        public virtual User? User { get; set; }
    }
}
