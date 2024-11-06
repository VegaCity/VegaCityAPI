using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class StoreService
    {
        public StoreService()
        {
            OrderDetails = new HashSet<OrderDetail>();
            WalletTypeStoreServiceMappings = new HashSet<WalletTypeStoreServiceMapping>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid StoreId { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public bool Deflag { get; set; }
        public int Price { get; set; }

        public virtual Store Store { get; set; } = null!;
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<WalletTypeStoreServiceMapping> WalletTypeStoreServiceMappings { get; set; }
    }
}
