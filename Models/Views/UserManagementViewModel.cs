using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using NetCoreBootstrap.Models.Database;

namespace NetCoreBootstrap.Models.Views
{
    public class UserManagementViewModel
    {
        public string Name { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string NewRole { get; set; }
        public List<SelectListItem> RolesListItem { get; set; }
        public List<User> Users { get; set; }
        public List<IdentityRole> Roles { get; set; }
    }
}