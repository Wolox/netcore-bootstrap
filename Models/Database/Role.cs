using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace NetCoreBootstrap.Models.Database
{
    public class Role : IdentityRole
    {
        public Role() : base() {}
        
        public Role(string role) : base(role) {}

        public virtual ICollection<UserRoles> UserRoles { get; set; }
    }
}
