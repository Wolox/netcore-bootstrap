using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetCoreBootstrap.Models.Database;
using NetCoreBootstrap.Models.Views;
using NetCoreBootstrap.Persistance.Repositories;

namespace NetCoreBootstrap.Controllers
{
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
        }

        public SignInManager<User> SignInManager
        {
            get { return this._signInManager; }
        }

        public UserManager<User> UserManager
        {
            get { return this._userManager; }
        }

        [AllowAnonymous]
        [HttpGet("Register")]
        public IActionResult Register() => View();

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserViewModel userViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = new User { UserName = userViewModel.UserName, Email = userViewModel.Email, IsExternal = false };
                var result = await UserManager.CreateAsync(user, userViewModel.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, true);
                    return RedirectToAction("Users", "UserManagement");
                }
                else foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(userViewModel);
        }

        [AllowAnonymous]
        [HttpGet("Login")]
        public async Task<IActionResult> Login()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            var viewModel = new LoginViewModel();
            viewModel.LoginProviders = (await SignInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            return View(viewModel);
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (ModelState.IsValid)
            {
                var result = await SignInManager.PasswordSignInAsync(loginViewModel.UserName, loginViewModel.Password, loginViewModel.RememberMe, false);
                if (result.Succeeded) return RedirectToAction("Users", "UserManagement");
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            loginViewModel.LoginProviders = (await SignInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            return View(loginViewModel);
        }

        [AllowAnonymous]
        [HttpPost("ExternalLogin")]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { ReturnUrl = returnUrl });
            var properties = SignInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [AllowAnonymous]
        [HttpGet("ExternalLoginCallback")]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View(nameof(Login));
            }
            var info = await SignInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction("Login");
            }

            var user = await UserManager.FindByEmailAsync(info.Principal.FindFirstValue(ClaimTypes.Email));
            if (user != null && !user.IsExternal)
            {
                // Already exists a local user with the external login mail.
                throw new Exception($"This email {user.Email} is already registered.");
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await SignInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded) return RedirectToAction("Users", "UserManagement");
            else if (result.IsLockedOut || result.IsNotAllowed || result.RequiresTwoFactor) throw new Exception(result.ToString());
            else if (await ConfirmExternalLogin(new UserManagementViewModel { Email = info.Principal.FindFirstValue(ClaimTypes.Email) }))
                return RedirectToAction("Users", "UserManagement");
            else return RedirectToAction("Login");
        }

        [HttpGet("Edit")]
        [Authorize]
        public async Task<IActionResult> Edit()
        {
            var user = await UserManager.GetUserAsync(HttpContext.User);
            if (user.IsExternal) return RedirectToAction("AccessDenied");
            var userViewModel = new UserManagementViewModel { UserId = user.Id };
            return View(userViewModel);
        }

        [HttpPost("Edit")]
        [Authorize]
        public async Task<IActionResult> Edit(UserManagementViewModel viewModel)
        {
            var user = await UserManager.GetUserAsync(HttpContext.User);
            if (ModelState.IsValid)
            {
                var result = await UserManager.ChangePasswordAsync(user, viewModel.Password, viewModel.NewPassword);
                if (result.Succeeded) return RedirectToAction("Users", "UserManagement");
                foreach (var error in result.Errors) ModelState.AddModelError(error.Code, error.Description);
            }
            return View(viewModel);
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await SignInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet("AccessDenied")]
        public IActionResult AccessDenied() => View();

        private async Task<bool> ConfirmExternalLogin(UserManagementViewModel viewModel)
        {
            var info = await SignInManager.GetExternalLoginInfoAsync();
            if (info == null) return false;
            var user = new User { UserName = viewModel.Email, Email = viewModel.Email, IsExternal = true };
            var result = await UserManager.CreateAsync(user);
            if (result.Succeeded)
            {
                result = await UserManager.AddLoginAsync(user, info);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false);
                    return true;
                }
            }
            return false;
        }
    }
}
