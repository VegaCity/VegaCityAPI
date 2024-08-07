using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Product
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid? ProductCategoryId { get; set; }
        public int? Amount { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }
        public bool? Deflag { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Size { get; set; }

        public virtual ProductCategory? ProductCategory { get; set; }
    }
}
