using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace NetCoreBootstrap.Models.Database
{
    public class User : IdentityUser
    {
        public bool IsExternal { get; set; }
        public virtual ICollection<IdentityRole> Roles { get; set; }
    }
}
