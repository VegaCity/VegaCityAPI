using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class PackageItem
    {
        public PackageItem()
        {
            CustomerMoneyTransfers = new HashSet<CustomerMoneyTransfer>();
            Deposits = new HashSet<Deposit>();
            Orders = new HashSet<Order>();
            Reports = new HashSet<Report>();
        }

        public Guid Id { get; set; }
        public Guid PackageId { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public string? Name { get; set; }
        public string? Cccdpassport { get; set; }
        public string? Email { get; set; }
        public string? Status { get; set; }
        public string? Gender { get; set; }
        public bool? IsAdult { get; set; }
        public Guid? WalletId { get; set; }

        public virtual Package Package { get; set; } = null!;
        public virtual Wallet? Wallet { get; set; }
        public virtual ICollection<CustomerMoneyTransfer> CustomerMoneyTransfers { get; set; }
        public virtual ICollection<Deposit> Deposits { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
    }
}
