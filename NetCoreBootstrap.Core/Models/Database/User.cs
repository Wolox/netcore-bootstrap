using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace NetCoreBootstrap.Core.Models.Database
{
    public class User : IdentityUser
    {
        public bool IsExternal { get; set; }
        public virtual ICollection<IdentityRole> Roles { get; set; }

        public bool IsEmailValid() =>
            new Regex("(@[a-zA-Z]{1,})(\\.[a-zA-Z]{2,}){1,2}").Match(Email).Success;
    }
}
