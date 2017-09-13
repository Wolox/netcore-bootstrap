using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NetCoreBootstrap.Models.Views
{
    public class RoleManagerViewModel
    {
        public List<SelectListItem> UsersListItem { get; set; }
        public List<string> CurrentRoles { get; set; }
        public Dictionary<string, bool> Roles { get; set; }
        public string SelectedUserId { get; set; }
    }
}
