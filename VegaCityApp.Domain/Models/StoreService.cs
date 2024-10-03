using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class StoreService
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid StoreId { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public bool Deflag { get; set; }
        public Guid? WalletTypeId { get; set; }

        public virtual Store Store { get; set; } = null!;
        public virtual WalletType? WalletType { get; set; }
    }
}
