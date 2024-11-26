using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class PackageOrder
    {
        public PackageOrder()
        {
            CustomerMoneyTransfers = new HashSet<CustomerMoneyTransfer>();
            Orders = new HashSet<Order>();
            Reports = new HashSet<Report>();
            Wallets = new HashSet<Wallet>();
        }

        public Guid Id { get; set; }
        public Guid? PackageId { get; set; }
        public string? VcardId { get; set; }
        public string CusName { get; set; } = null!;
        public string CusEmail { get; set; } = null!;
        public string CusCccdpassport { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsAdult { get; set; }
        public Guid? CustomerId { get; set; }
        public bool IsChangedInfo { get; set; }

        public virtual Customer? Customer { get; set; }
        public virtual Package? Package { get; set; }
        public virtual Vcard? Vcard { get; set; }
        public virtual ICollection<CustomerMoneyTransfer> CustomerMoneyTransfers { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
        public virtual ICollection<Wallet> Wallets { get; set; }
    }
}
