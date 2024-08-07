using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class StoreSession
    {
        public Guid Id { get; set; }
        public DateTime? StDate { get; set; }
        public DateTime? EDate { get; set; }
        public int? NumberOfOrder { get; set; }
        public int? TotalAmount { get; set; }
        public int? TotalProduct { get; set; }
        public Guid? StoreId { get; set; }

        public virtual Store? Store { get; set; }
    }
}
