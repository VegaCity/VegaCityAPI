using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class IssueType
    {
        public IssueType()
        {
            DisputeReports = new HashSet<DisputeReport>();
        }

        public Guid Id { get; set; }
        public string? Name { get; set; }
        public DateTime? CrDate { get; set; }

        public virtual ICollection<DisputeReport> DisputeReports { get; set; }
    }
}
