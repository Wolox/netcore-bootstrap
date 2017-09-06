using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NetCoreBootstrap.Models.Database;

namespace NetCoreBootstrap.Models.Views
{
    public class UserManagementUsersViewModel
    {
        public List<User> Users { get; set; }
    }
}