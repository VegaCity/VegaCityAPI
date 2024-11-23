using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Report
    {
        public Guid Id { get; set; }
        public Guid? CreatorPackageOrderId { get; set; }
        public Guid? CreatorStoreId { get; set; }
        public Guid? SolveUserId { get; set; }
        public Guid IssueTypeId { get; set; }
        public string Description { get; set; } = null!;
        public string? Solution { get; set; }
        public string? Creator { get; set; }
        public string? SolveBy { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public int Status { get; set; }

        public virtual PackageOrder? CreatorPackageOrder { get; set; }
        public virtual Store? CreatorStore { get; set; }
        public virtual IssueType IssueType { get; set; } = null!;
        public virtual User? SolveUser { get; set; }
    }
}
