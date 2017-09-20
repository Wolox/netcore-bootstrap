using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace NetCoreBootstrap.Models.Database
{
    public class UserRole : IdentityUserRole<string>
    {
        public UserRole() : base(){}

        public virtual Role Role { get; set; }
        public virtual User User { get; set; }
    }
}
