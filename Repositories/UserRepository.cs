using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NetCoreBootstrap.Models.Database;

namespace NetCoreBootstrap.Repositories
{
    public class UserRepository
    {
        private readonly DbContextOptions<DataBaseContext> _options;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public UserRepository(DbContextOptions<DataBaseContext> options, UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            this._options = options;
            this._userManager = userManager;
            this._roleManager = roleManager;
        }

        public async Task<User> GetUserById(string id)
        {
            var user = await UserManager.FindByIdAsync(id);
            user.UserRoles = new List<UserRoles>();
            using(var context = Context)
            {
                var roles = from role in context.UserRoles where role.UserId.Equals(user.Id) select role;
                foreach(var role in roles)
                {
                    var userRole = new UserRoles { UserId = user.Id, RoleId = role.RoleId };
                    user.UserRoles.Add(userRole);
                }
                return user;
            }
        }

        public async Task<List<User>> GetAllUsers()
        {
            using(var context = Context)
            {
                var users = context.Users;
                foreach(var user in users)
                {
                    context.Entry(user).Collection(u => u.UserRoles).Load();
                    for(var i = 0; i < user.UserRoles.Count; i++)
                    {
                       user.UserRoles.ElementAt(i).Role = await GetRoleById(user.UserRoles.ElementAt(i).RoleId);
                    }
                }
                return users.ToList();
            }
        }

        public List<Role> GetAllRoles()
        {
            return RoleManager.Roles.ToList();
        }

        public List<SelectListItem> GetRoles()
        {
            var roles = new List<SelectListItem>();
            foreach(var role in RoleManager.Roles.OrderBy(r => r.Name).ToList())
            {
                roles.Add(new SelectListItem { Text = role.Name, Value = role.Name });
            }
            return roles;
        }

        public async Task<IdentityResult> AddRoleToUser(User user, string role)
        {
            return await UserManager.AddToRoleAsync(user, role);
        }

        public async Task<IdentityResult> RemoveRoleFromUser(User user, string role)
        {
            return await UserManager.RemoveFromRoleAsync(user, role);
        }

        public async Task<IdentityResult> CreateRole(string role)
        {
            return await RoleManager.CreateAsync(new Role(role));
        }

        public async Task<bool> DeleteRole(string roleId)
        {
            try 
            {
                var role = await RoleManager.FindByIdAsync(roleId);
                await RoleManager.DeleteAsync(role);
            }
            catch(Exception)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> UpdateRole(string roleId, string name)
        {
            try
            {
                var role = await GetRoleById(roleId);
                role.Name = name;
                return (await RoleManager.UpdateAsync(role)).Succeeded;
            }
            catch(Exception)
            {
                return false;
            }
		}

        public async Task<Role> GetRoleById(string roleId)
        {
            try
            {
                return await RoleManager.FindByIdAsync(roleId);
            }
            catch(Exception e)
            {
                throw new Exception(e.Message);
            }
		}

        public async Task<IEnumerable<string>> GetRoles(User user)
        {
            return await UserManager.GetRolesAsync(user);
        }

        public List<SelectListItem> GetUsersListItem()
        {
            var users = new List<SelectListItem>();
            foreach(var user in UserManager.Users.OrderBy(u => u.Email).ToList())
            {
                users.Add(new SelectListItem { Text = user.Email, Value = user.Id });
            }
            return users;
		}
        
        public UserManager<User> UserManager
        {
            get { return _userManager; }
        }

        public RoleManager<Role> RoleManager
        {
            get { return _roleManager; }
        }

        public DataBaseContext Context
        {
            get { return new DataBaseContext(_options); }
        }
	}
}
