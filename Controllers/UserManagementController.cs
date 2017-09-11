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

namespace NetCoreBootstrap.Controllers
{
    [Route("[controller]"), Authorize]
    public class UserManagementController : Controller
    {
        private readonly DbContextOptions<DataBaseContext> _options;
        private readonly UserRepository _userRepository;

        public UserManagementController(DbContextOptions<DataBaseContext> options, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            this._options = options;
            this._userRepository = new UserRepository(this._options, userManager, roleManager);
        }

        [HttpGet("Users")]
        public IActionResult Users() => View(new UserManagementViewModel { Users = UserRepository.GetAllUsers() });

        [HttpGet("Roles")]
        public IActionResult Roles() => View(new UserManagementViewModel { Roles = UserRepository.GetAllRoles() });

        [HttpGet("AddRole")]
        public async Task<IActionResult> AddRole(string userId)
        {
            var user = await UserRepository.GetUserById(userId);
            return View(new UserManagementViewModel { UserId = user.Id, Email = user.Email, RolesListItem = UserRepository.GetRoles() });
        }

        [HttpPost("AddRole")]
        public async Task<IActionResult> AddRole(UserManagementViewModel viewModel)
        {
            var user = await UserRepository.GetUserById(viewModel.UserId);
            if(ModelState.IsValid)
            {
                var result = await UserRepository.AddRoleToUser(user, viewModel.NewRole);
                if(result.Succeeded) return RedirectToAction("Users");
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
            }
            viewModel.RolesListItem = UserRepository.GetRoles();
            viewModel.Email = user.Email;
            return View(viewModel);
        }

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

        public DbContextOptions<DataBaseContext> Options
        {
            get { return _options; }
        }

        public UserRepository UserRepository
        {
            get { return _userRepository; }
        }
    }
}
