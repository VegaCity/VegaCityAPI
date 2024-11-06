using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class PackageOrder
    {
        public Guid Id { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? PackageId { get; set; }
        public string? CusName { get; set; }
        public string? CusCccdpassport { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CusEmail { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }
        public string? Status { get; set; }

        public virtual Order? Order { get; set; }
        public virtual Package? Package { get; set; }
    }
}
