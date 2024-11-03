﻿using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Report
    {
        public Guid Id { get; set; }
        public Guid? PackageItemId { get; set; }
        public Guid? UserId { get; set; }
        public Guid IssueTypeId { get; set; }
        public string Description { get; set; } = null!;
        public string? Solution { get; set; }
        public string? SolveBy { get; set; }
        public Guid CrDate { get; set; }
        public Guid UpsDate { get; set; }
        public int Status { get; set; }

        public virtual IssueType IssueType { get; set; } = null!;
        public virtual PackageItem? PackageItem { get; set; }
        public virtual User? User { get; set; }
    }
}