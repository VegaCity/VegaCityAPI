﻿using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public bool Deflag { get; set; }

        public virtual User? User { get; set; }
    }
}
