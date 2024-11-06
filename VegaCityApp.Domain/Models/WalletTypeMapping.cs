using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class WalletTypeMapping
    {
        public Guid Id { get; set; }
        public Guid ProductCategoryId { get; set; }
        public Guid WalletTypeId { get; set; }

        public virtual ProductCategory ProductCategory { get; set; } = null!;
        public virtual WalletType WalletType { get; set; } = null!;
    }
}
