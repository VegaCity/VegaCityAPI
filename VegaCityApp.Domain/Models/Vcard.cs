using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Vcard
    {
        public Vcard()
        {
            PackageOrders = new HashSet<PackageOrder>();
        }

        public string Id { get; set; } = null!;
        public string? Name { get; set; }
        public string? Cccdpassport { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Status { get; set; }
        public bool? IsAdult { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ImageUrl { get; set; }

        public virtual ICollection<PackageOrder> PackageOrders { get; set; }
    }
}
