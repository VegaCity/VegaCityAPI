using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Account
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public DateTime? CrDate { get; set; }
        public DateTime? UpsDate { get; set; }
        public string? Status { get; set; }
        public Guid? UserId { get; set; }
        public Guid? RoleId { get; set; }

        public virtual Role? Role { get; set; }
        public virtual User? User { get; set; }
    }
}
