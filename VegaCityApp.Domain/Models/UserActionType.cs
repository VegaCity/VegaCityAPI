using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class UserActionType
    {
        public UserActionType()
        {
            UserActions = new HashSet<UserAction>();
        }

        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid? MarketZoneId { get; set; }

        public virtual MarketZone? MarketZone { get; set; }
        public virtual ICollection<UserAction> UserActions { get; set; }
    }
}
