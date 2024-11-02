using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class UserStoreMapping
    {
        public Guid Id { get; set; }
        public Guid? StoreId { get; set; }
        public Guid? UserId { get; set; }

        public virtual Store? Store { get; set; }
        public virtual User? User { get; set; }
    }
}
