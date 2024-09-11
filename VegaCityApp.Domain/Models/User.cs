using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class User
    {
        public User()
        {
            DisputeReports = new HashSet<DisputeReport>();
            Wallets = new HashSet<Wallet>();
        }

        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public DateTime? Birthday { get; set; }
        public Guid? StoreId { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public int Gender { get; set; }
        public string Cccd { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string? PinCode { get; set; }
        public Guid MarketZoneId { get; set; }
        public string Email { get; set; } = null!;
        public string? Password { get; set; }
        public Guid RoleId { get; set; }
        public string? Description { get; set; }
        public bool? IsChange { get; set; }
        public string? Address { get; set; }
        public int Status { get; set; }

        public virtual Role Role { get; set; } = null!;
        public virtual Store? Store { get; set; }
        public virtual ICollection<DisputeReport> DisputeReports { get; set; }
        public virtual ICollection<Wallet> Wallets { get; set; }
    }
}
