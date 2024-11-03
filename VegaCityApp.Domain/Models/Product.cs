﻿using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Product
    {
        public Product()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid? ProductCategoryId { get; set; }
        public Guid? MenuId { get; set; }
        public int? Price { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }
        public string? Status { get; set; }
        public string? ImageUrl { get; set; }

        public virtual Menu? Menu { get; set; }
        public virtual ProductCategory? ProductCategory { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}