using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Hash
    {
        public string Key { get; set; } = null!;
        public string Field { get; set; } = null!;
        public string? Value { get; set; }
        public DateTime? ExpireAt { get; set; }
    }
}
