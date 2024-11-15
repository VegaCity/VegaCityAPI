using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class PackageOrder
    {
        public PackageOrder()
        {
            CustomerMoneyTransfers = new HashSet<CustomerMoneyTransfer>();
            Wallets = new HashSet<Wallet>();
        }

        public Guid Id { get; set; }
        public Guid PackageId { get; set; }
        public string? VcardId { get; set; }
        public string CusName { get; set; } = null!;
        public string CusEmail { get; set; } = null!;
        public string CusCccdpassport { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public string Status { get; set; } = null!;

        public virtual Package Package { get; set; } = null!;
        public virtual Vcard? Vcard { get; set; }
        public virtual ICollection<CustomerMoneyTransfer> CustomerMoneyTransfers { get; set; }
        public virtual ICollection<Wallet> Wallets { get; set; }
    }
}
