using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Customer
    {
        public Customer()
        {
            Orders = new HashSet<Order>();
            PackageOrders = new HashSet<PackageOrder>();
        }

        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Cccdpassport { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<PackageOrder> PackageOrders { get; set; }
    }
}
