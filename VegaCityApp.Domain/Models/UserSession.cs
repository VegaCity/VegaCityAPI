using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class UserSession
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalCashReceive { get; set; }
        public int TotalFinalAmountOrder { get; set; }
        public int TotalQuantityOrder { get; set; }
        public int TotalWithrawCash { get; set; }
        public Guid ZoneId { get; set; }
        public string Status { get; set; } = null!;

        public virtual User User { get; set; } = null!;
        public virtual Zone Zone { get; set; } = null!;
    }
}
