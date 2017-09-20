using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NetCoreBootstrap.Models.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using NetCoreBootstrap.Models.Views;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using NetCoreBootstrap.Repositories;
using Microsoft.AspNetCore.Mvc.Localization;

namespace NetCoreBootstrap.Controllers
{
    [Route("[controller]"), Authorize]
    public class UserManagementController : Controller
    {
        private readonly DbContextOptions<DataBaseContext> _options;
		private readonly IHtmlLocalizer<UserManagementController> _localizer;
		private readonly UserRepository _userRepository;

        public UserManagementController(DbContextOptions<DataBaseContext> options, 
                                        UserManager<User> userManager, 
                                        RoleManager<Role> roleManager, 
                                        IHtmlLocalizer<UserManagementController> localizer)
        {
            this._options = options;
			this._localizer = localizer;
			this._userRepository = new UserRepository(this._options, userManager, roleManager);
        }

        [HttpGet("Users")]
        public IActionResult Users()
        {
            var users = UserRepository.GetAllUsersWithRoles();
            var roleList = new List<string>();
            foreach(var user in users)
            {
                foreach(var role in user.Roles)
                {
                    //roleList.Add(role.Role.Name);
                }
            }
            return View(new UserManagementViewModel { Users = users });
        }

        [HttpGet("Roles")]
        public IActionResult Roles() => View(new UserManagementViewModel { Roles = UserRepository.GetAllRoles() });

        [HttpGet("NewRole")]
        public IActionResult CreateRole() => View();

        [HttpPost("NewRole")]
        public async Task<IActionResult> CreateRole(UserManagementViewModel viewModel)
        {
            if(ModelState.IsValid)
            {
                var result = await UserRepository.CreateRole(viewModel.Name);
                if(result.Succeeded) return RedirectToAction("Roles");
                foreach(var error in result.Errors) ModelState.AddModelError(error.Code, error.Description);
            }
            return View(viewModel);
        }

        [HttpGet("DeleteRole")]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            var result = await UserRepository.DeleteRole(roleId);
            if(result) ViewData["Message"] = Localizer["RoleDeleted"];
            else ViewData["Message"] = Localizer["RoleNotDeleted"];
            return View("./Views/UserManagement/Roles.cshtml", new UserManagementViewModel { Roles = UserRepository.GetAllRoles() });
        }

        [HttpGet("EditRole")]
        public async Task<IActionResult> EditRole(string roleId)
        {
            var role = await UserRepository.GetRoleById(roleId);
            var roleViewModel = new UserManagementViewModel { RoleId = role.Id, Name = role.Name };
            return View(roleViewModel);
        }

        [HttpPost("EditRole")]
        public async Task<IActionResult> EditRole(UserManagementViewModel viewModel)
        {
            var result = await UserRepository.UpdateRole(viewModel.RoleId, viewModel.Name);
            if(result) ViewData["Message"] = Localizer["RoleUpdated"];
            else ViewData["Message"] = Localizer["RoleNotUpdated"];
            return View("./Views/UserManagement/Roles.cshtml", new UserManagementViewModel { Roles = UserRepository.GetAllRoles() });
        }

        [HttpGet("RoleManager")]
        public IActionResult RoleManager()
        {
            return View(new RoleManagerViewModel { UsersListItem = UserRepository.GetUsersListItem(), Roles = new Dictionary<string, bool>() });
        }

        [HttpGet("ViewRoles")]
        public async Task<IActionResult> ViewRoles(string userId)
        {
            var viewModel = new RoleManagerViewModel { Roles = new Dictionary<string, bool>() };
            var user = await UserRepository.GetUserById(userId);
            foreach(var role in UserRepository.GetAllRoles())
            {
                viewModel.Roles[role.ToString()] = await UserRepository.IsUserInRole(user, role.ToString());
            }
            viewModel.SelectedUserId = userId;
            return PartialView("_UserRoles", viewModel);
        }

        [HttpPost("AddRolesToUser")]
        public async Task<IActionResult> AddRolesToUser(RoleManagerViewModel viewModel)
        {
            var user = await UserRepository.GetUserById(viewModel.SelectedUserId);
            foreach(var role in viewModel.Roles)
            {
                if(await UserRepository.IsUserInRole(user, role.Key) && !role.Value)
                {
                    await UserRepository.RemoveRoleFromUser(user, role.Key);
                }
                else if(role.Value) await UserRepository.AddRoleToUser(user, role.Key);
            }
            return RedirectToAction("RoleManager");
        }

        public DbContextOptions<DataBaseContext> Options
        {
            get { return _options; }
        }

        public UserRepository UserRepository
        {
            get { return _userRepository; }
        }

        public IHtmlLocalizer<UserManagementController> Localizer
        {
            get { return _localizer; }
        }
    }
}
