using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class PackageDetail
    {
        public Guid Id { get; set; }
        public Guid? PackageId { get; set; }
        public Guid? WalletTypeId { get; set; }
        public int? StartMoney { get; set; }
        public DateTime? CrDate { get; set; }

        public virtual Package? Package { get; set; }
        public virtual WalletType? WalletType { get; set; }
    }
}
