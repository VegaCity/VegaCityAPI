using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class UserRefreshToken
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string? Token { get; set; }
        public string? Name { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }

        public virtual User? User { get; set; }
    }
}
