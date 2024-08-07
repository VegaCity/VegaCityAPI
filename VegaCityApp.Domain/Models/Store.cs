using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Store
    {
        public Store()
        {
            Menus = new HashSet<Menu>();
            OwnerStores = new HashSet<OwnerStore>();
            StoreSessions = new HashSet<StoreSession>();
            Transactions = new HashSet<Transaction>();
            UserActions = new HashSet<UserAction>();
        }

        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Status { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }
        public bool? Deflag { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ShortName { get; set; }
        public string? Email { get; set; }
        public Guid? MarketZoneId { get; set; }
        public string? Description { get; set; }

        public virtual MarketZone? MarketZone { get; set; }
        public virtual ICollection<Menu> Menus { get; set; }
        public virtual ICollection<OwnerStore> OwnerStores { get; set; }
        public virtual ICollection<StoreSession> StoreSessions { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<UserAction> UserActions { get; set; }
    }
}
