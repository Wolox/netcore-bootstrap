using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using NetCoreBootstrap.Models.Database;

namespace NetCoreBootstrap.Models.Views
{
    public class UserManagementAddRoleViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string NewRole { get; set; }
        public List<SelectListItem> Roles { get; set; }
    }
}