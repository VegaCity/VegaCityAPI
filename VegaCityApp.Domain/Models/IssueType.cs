using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class IssueType
    {
        public IssueType()
        {
            Reports = new HashSet<Report>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CrDate { get; set; }

        public virtual ICollection<Report> Reports { get; set; }
    }
}
