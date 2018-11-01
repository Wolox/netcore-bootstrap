using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using NetCoreBootstrap.Models.Database;

namespace NetCoreBootstrap.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUserById(string id);
        Task<bool> IsUserInRole(User user, string role);
        List<User> GetAllUsers();
        List<User> GetAllUsersWithRoles();
        List<IdentityRole> GetAllRoles();
        Task<IdentityResult> AddRoleToUser(User user, string role);
        Dictionary<string, string> GetRoleMap();
        Task<IdentityResult> RemoveRoleFromUser(User user, string role);
        Task<IdentityResult> CreateRole(string role);
        Task<bool> DeleteRole(string roleId);
        Task<bool> UpdateRole(string roleId, string name);
        Task<IdentityRole> GetRoleById(string roleId);
        Task<IEnumerable<string>> GetRoles(User user);
        List<SelectListItem> GetUsersListItem();
    }
}
