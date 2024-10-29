using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Etag
    {
        public Etag()
        {
            Deposits = new HashSet<Deposit>();
            Orders = new HashSet<Order>();
        }

        public Guid Id { get; set; }
        public string? ImageUrl { get; set; }
        public string Qrcode { get; set; } = null!;
        public string EtagCode { get; set; } = null!;
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public bool Deflag { get; set; }
        public Guid EtagTypeId { get; set; }
        public Guid MarketZoneId { get; set; }
        public Guid WalletId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Status { get; set; }
        public bool? IsAdult { get; set; }

        public virtual EtagType EtagType { get; set; } = null!;
        public virtual MarketZone MarketZone { get; set; } = null!;
        public virtual Wallet Wallet { get; set; } = null!;
        public virtual EtagDetail? EtagDetail { get; set; }
        public virtual ICollection<Deposit> Deposits { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
