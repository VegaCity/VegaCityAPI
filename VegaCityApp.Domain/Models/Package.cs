using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Package
    {
        public Package()
        {
            OrderDetails = new HashSet<OrderDetail>();
            Orders = new HashSet<Order>();
            PackageDetails = new HashSet<PackageDetail>();
            PackageOrders = new HashSet<PackageOrder>();
        }

        public Guid Id { get; set; }
        public string Type { get; set; } = null!;
        public Guid ZoneId { get; set; }
        public string? ImageUrl { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int Price { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public bool Deflag { get; set; }
        public int Duration { get; set; }

        public virtual Zone Zone { get; set; } = null!;
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<PackageDetail> PackageDetails { get; set; }
        public virtual ICollection<PackageOrder> PackageOrders { get; set; }
    }
}
