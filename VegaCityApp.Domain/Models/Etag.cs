using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Etag
    {
        public Etag()
        {
            Deposits = new HashSet<Deposit>();
            ENotifications = new HashSet<ENotification>();
            Orders = new HashSet<Order>();
        }

        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Cccd { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public int Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Qrcode { get; set; }
        public string? EtagCode { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public bool Deflag { get; set; }
        public Guid EtagTypeId { get; set; }
        public Guid MarketZoneId { get; set; }
        public Guid WalletId { get; set; }

        public virtual EtagType EtagType { get; set; } = null!;
        public virtual MarketZone MarketZone { get; set; } = null!;
        public virtual Wallet Wallet { get; set; } = null!;
        public virtual ICollection<Deposit> Deposits { get; set; }
        public virtual ICollection<ENotification> ENotifications { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
