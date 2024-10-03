using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class WalletTypeStoreServiceMapping
    {
        public Guid Id { get; set; }
        public Guid WalletTypeId { get; set; }
        public Guid StoreServiceId { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }

        public virtual StoreService StoreService { get; set; } = null!;
        public virtual WalletType WalletType { get; set; } = null!;
    }
}
