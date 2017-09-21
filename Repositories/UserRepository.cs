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
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRepository(DbContextOptions<DataBaseContext> options, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            this._options = options;
            this._userManager = userManager;
            this._roleManager = roleManager;
        }

        public async Task<User> GetUserById(string id)
        {
            return await UserManager.FindByIdAsync(id);
        }

        public async Task<bool> IsUserInRole(User user, string role)
        {
            return await UserManager.IsInRoleAsync(user, role);
        }

        public List<User> GetAllUsers()
        {
            return UserManager.Users.ToList();
        }

        public List<User> GetAllUsersWithRoles()
        {
            using(var context = Context)
            {
                return (from user in context.Users
                        select new User {
                            Id = user.Id,
                            Email = user.Email,
                            UserName = user.UserName,
                            Roles = (from role in context.Roles 
                                        join userRole in context.UserRoles on role.Id equals userRole.RoleId
                                        where userRole.UserId == user.Id
                                        select role).ToList()
                        }).Distinct().ToList();
            }
        }

        public List<IdentityRole> GetAllRoles()
        {
            return RoleManager.Roles.ToList();
        }

        public async Task<IdentityResult> AddRoleToUser(User user, string role)
        {
            return await UserManager.AddToRoleAsync(user, role);
        }

        public Dictionary<string, string> GetRoleMap()
        {
            var map = new Dictionary<string, string>();
            foreach(var role in GetAllRoles())
            {
                map[role.Id] = role.Name;
            }
            return map;
        }

        public async Task<IdentityResult> RemoveRoleFromUser(User user, string role)
        {
            return await UserManager.RemoveFromRoleAsync(user, role);
        }

        public async Task<IdentityResult> CreateRole(string role)
        {
            return await RoleManager.CreateAsync(new IdentityRole(role));
        }

        public async Task<bool> DeleteRole(string roleId)
        {
            var role = await RoleManager.FindByIdAsync(roleId);
            try 
            {
                return (await RoleManager.DeleteAsync(role)).Succeeded;
            }
            catch(Exception)
            {
                return false;
            }
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

        public async Task<IdentityRole> GetRoleById(string roleId)
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
            return (from user in UserManager.Users.OrderBy(u => u.Email) select new SelectListItem { Text = user.Email, Value = user.Id }).ToList();
        }
        
        public UserManager<User> UserManager
        {
            get { return _userManager; }
        }

        public RoleManager<IdentityRole> RoleManager
        {
            get { return _roleManager; }
        }

        public DataBaseContext Context
        {
            get { return new DataBaseContext(_options); }
        }
	}
}
