using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using NetCoreBootstrap.Models.Database;

namespace NetCoreBootstrap.Models.Views
{
    public class UserManagementRolesViewModel
    {
        public List<IdentityRole> Roles { get; set; }
    }
}