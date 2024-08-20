using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Etag
    {
        public Etag()
        {
            Orders = new HashSet<Order>();
            Transactions = new HashSet<Transaction>();
            UserWallets = new HashSet<UserWallet>();
        }

        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public Guid? MarketZoneId { get; set; }
        public int? Balance { get; set; }
        public string? Cccd { get; set; }
        public string? ImageUrl { get; set; }
        public int? Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Qrcode { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }
        public bool? Deflag { get; set; }
        public Guid? EtagTypeId { get; set; }

        public virtual EtagType? EtagType { get; set; }
        public virtual MarketZone? MarketZone { get; set; }
        public virtual User? User { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<UserWallet> UserWallets { get; set; }
    }
}
