using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore;
using NetCoreBootstrap.Models.Database;
using NetCoreBootstrap.Models.Views;
using NetCoreBootstrap.Repositories;
using NetCoreBootstrap.Repositories.Database;
using NetCoreBootstrap.Repositories.Interfaces;

namespace NetCoreBootstrap.Controllers
{
    [Route("[controller]")]
    [Authorize]
    public class UserManagementController : Controller
    {
        private readonly IHtmlLocalizer<UserManagementController> _localizer;
        private readonly IUnitOfWork _unitOfWork;

        public UserManagementController(DataBaseContext context,
                                        UserManager<User> userManager,
                                        RoleManager<IdentityRole> roleManager,
                                        IHtmlLocalizer<UserManagementController> localizer)
        {
            this._localizer = localizer;
            this._unitOfWork = new UnitOfWork(context, userManager, roleManager);
        }

        public IUnitOfWork UnitOfWork { get => this._unitOfWork; }

        public IHtmlLocalizer<UserManagementController> Localizer { get => this._localizer; }

        [HttpGet("Users")]
        public IActionResult Users()
        {
            return View(new UserManagementViewModel { Users = UnitOfWork.UserRepository.GetAllUsersWithRoles() });
        }

        [HttpGet("Roles")]
        public IActionResult Roles() => View(new UserManagementViewModel { Roles = UnitOfWork.UserRepository.GetAllRoles() });

        [HttpGet("NewRole")]
        public IActionResult CreateRole() => View();

        [HttpPost("NewRole")]
        public async Task<IActionResult> CreateRole(UserManagementViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var result = await UnitOfWork.UserRepository.CreateRole(viewModel.Name);
                if (result.Succeeded) return RedirectToAction("Roles");
                foreach (var error in result.Errors) ModelState.AddModelError(error.Code, error.Description);
            }
            return View(viewModel);
        }

        [HttpGet("DeleteRole")]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            var result = await UnitOfWork.UserRepository.DeleteRole(roleId);
            if (result) ViewData["Message"] = Localizer["RoleDeleted"];
            else ViewData["Message"] = Localizer["RoleNotDeleted"];
            return View("./Views/UserManagement/Roles.cshtml", new UserManagementViewModel { Roles = UnitOfWork.UserRepository.GetAllRoles() });
        }

        [HttpGet("EditRole")]
        public async Task<IActionResult> EditRole(string roleId)
        {
            var role = await UnitOfWork.UserRepository.GetRoleById(roleId);
            var roleViewModel = new UserManagementViewModel { RoleId = role.Id, Name = role.Name };
            return View(roleViewModel);
        }

        [HttpPost("EditRole")]
        public async Task<IActionResult> EditRole(UserManagementViewModel viewModel)
        {
            var result = await UnitOfWork.UserRepository.UpdateRole(viewModel.RoleId, viewModel.Name);
            if (result) ViewData["Message"] = Localizer["RoleUpdated"];
            else ViewData["Message"] = Localizer["RoleNotUpdated"];
            return View("./Views/UserManagement/Roles.cshtml", new UserManagementViewModel { Roles = UnitOfWork.UserRepository.GetAllRoles() });
        }

        [HttpGet("RoleManager")]
        public IActionResult RoleManager()
        {
            return View(new RoleManagerViewModel { UsersListItem = UnitOfWork.UserRepository.GetUsersListItem(), Roles = new Dictionary<string, bool>() });
        }

        [HttpGet("ViewRoles")]
        public async Task<IActionResult> ViewRoles(string userId)
        {
            var viewModel = new RoleManagerViewModel { Roles = new Dictionary<string, bool>() };
            var user = await UnitOfWork.UserRepository.GetUserById(userId);
            foreach (var role in UnitOfWork.UserRepository.GetAllRoles())
            {
                viewModel.Roles[role.ToString()] = await UnitOfWork.UserRepository.IsUserInRole(user, role.ToString());
            }
            viewModel.SelectedUserId = userId;
            return PartialView("_UserRoles", viewModel);
        }

        [HttpPost("AddRolesToUser")]
        public async Task<IActionResult> AddRolesToUser(RoleManagerViewModel viewModel)
        {
            var user = await UnitOfWork.UserRepository.GetUserById(viewModel.SelectedUserId);
            foreach (var role in viewModel.Roles)
            {
                if (await UnitOfWork.UserRepository.IsUserInRole(user, role.Key) && !role.Value) await UnitOfWork.UserRepository.RemoveRoleFromUser(user, role.Key);
                else if (role.Value) await UnitOfWork.UserRepository.AddRoleToUser(user, role.Key);
            }
            return RedirectToAction("RoleManager");
        }
    }
}
