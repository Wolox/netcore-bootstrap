using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace NetCoreBootstrap.Models.Database
{
    public class Role : IdentityRole
    {
        public Role() : base() {}
        public Role(string roleName) : base(roleName) {}

        public virtual ICollection<UserRole> Users { get; set; }
    }
}
