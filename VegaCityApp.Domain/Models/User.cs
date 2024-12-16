using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class User
    {
        public User()
        {
            BalanceEndDays = new HashSet<BalanceEndDay>();
            Orders = new HashSet<Order>();
            Reports = new HashSet<Report>();
            Transactions = new HashSet<Transaction>();
            UserRefreshTokens = new HashSet<UserRefreshToken>();
            UserSessions = new HashSet<UserSession>();
            UserStoreMappings = new HashSet<UserStoreMapping>();
            Wallets = new HashSet<Wallet>();
        }

        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public DateTime? Birthday { get; set; }
        public Guid? StoreId { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public int Gender { get; set; }
        public string CccdPassport { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public Guid MarketZoneId { get; set; }
        public string Email { get; set; } = null!;
        public string? Password { get; set; }
        public Guid RoleId { get; set; }
        public string? Description { get; set; }
        public bool IsChange { get; set; }
        public string Address { get; set; } = null!;
        public int Status { get; set; }
        public bool IsChangeInfo { get; set; }
        public int? RegisterStoreType { get; set; }
        public int? CountWrongPw { get; set; }

        public virtual MarketZone MarketZone { get; set; } = null!;
        public virtual Role Role { get; set; } = null!;
        public virtual ICollection<BalanceEndDay> BalanceEndDays { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<UserRefreshToken> UserRefreshTokens { get; set; }
        public virtual ICollection<UserSession> UserSessions { get; set; }
        public virtual ICollection<UserStoreMapping> UserStoreMappings { get; set; }
        public virtual ICollection<Wallet> Wallets { get; set; }
    }
}
