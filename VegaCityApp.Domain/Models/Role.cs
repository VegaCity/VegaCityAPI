using System;
using System.Collections.Generic;

namespace VegaCityApp.Domain.Models
{
    public partial class Role
    {
        public Role()
        {
            Users = new HashSet<User>();
        }

        public Guid Id { get; set; }
        public string? Name { get; set; }
        public bool? Deflag { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}
