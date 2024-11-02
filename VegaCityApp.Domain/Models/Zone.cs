using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Zone
    {
        public Zone()
        {
            UserSessions = new HashSet<UserSession>();
        }

        public Guid Id { get; set; }
        public Guid MarketZoneId { get; set; }
        public string Name { get; set; } = null!;
        public string Location { get; set; } = null!;
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public bool Deflag { get; set; }

        public virtual MarketZone MarketZone { get; set; } = null!;
        public virtual Store? Store { get; set; }
        public virtual ICollection<UserSession> UserSessions { get; set; }
    }
}
