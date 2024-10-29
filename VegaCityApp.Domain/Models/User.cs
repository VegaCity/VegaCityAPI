using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class User
    {
        public User()
        {
            Orders = new HashSet<Order>();
            UserRefreshTokens = new HashSet<UserRefreshToken>();
            Wallets = new HashSet<Wallet>();
        }

        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public DateTime? Birthday { get; set; }
        public Guid? StoreId { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public int Gender { get; set; }
        public string? CccdPassport { get; set; }
        public string? ImageUrl { get; set; }
        public Guid MarketZoneId { get; set; }
        public string Email { get; set; } = null!;
        public string? Password { get; set; }
        public Guid RoleId { get; set; }
        public string? Description { get; set; }
        public bool? IsChange { get; set; }
        public string? Address { get; set; }
        public int Status { get; set; }

        public virtual MarketZone MarketZone { get; set; } = null!;
        public virtual Role Role { get; set; } = null!;
        public virtual Store? Store { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<UserRefreshToken> UserRefreshTokens { get; set; }
        public virtual ICollection<Wallet> Wallets { get; set; }
    }
}
