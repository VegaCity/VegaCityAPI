using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class User
    {
        public User()
        {
            Accounts = new HashSet<Account>();
            OwnerStores = new HashSet<OwnerStore>();
            UserWallets = new HashSet<UserWallet>();
        }

        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? Birthday { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }
        public int? Gender { get; set; }
        public string? Cccd { get; set; }
        public string? ImageUrl { get; set; }
        public string? PinCode { get; set; }
        public Guid? MarketZoneId { get; set; }
        public string? Email { get; set; }

        public virtual ICollection<Account> Accounts { get; set; }
        public virtual ICollection<OwnerStore> OwnerStores { get; set; }
        public virtual ICollection<UserWallet> UserWallets { get; set; }
    }
}
