using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Store
    {
        public Store()
        {
            DisputeReports = new HashSet<DisputeReport>();
            Menus = new HashSet<Menu>();
            Orders = new HashSet<Order>();
            Transactions = new HashSet<Transaction>();
            Users = new HashSet<User>();
        }

        public Guid Id { get; set; }
        public int? StoreType { get; set; }
        public string Name { get; set; } = null!;
        public string? Address { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public bool Deflag { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ShortName { get; set; }
        public string? Email { get; set; }
        public Guid? HouseId { get; set; }
        public Guid MarketZoneId { get; set; }
        public string? Description { get; set; }
        public int? Status { get; set; }

        public virtual House? House { get; set; }
        public virtual MarketZone MarketZone { get; set; } = null!;
        public virtual ICollection<DisputeReport> DisputeReports { get; set; }
        public virtual ICollection<Menu> Menus { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
