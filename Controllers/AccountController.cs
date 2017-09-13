using Microsoft.AspNetCore.Mvc;
using NetCoreBootstrap.Models.Database;
using NetCoreBootstrap.Repositories;
using Microsoft.EntityFrameworkCore;
using NetCoreBootstrap.Models.Views;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System;
using Microsoft.Extensions.Options;

namespace NetCoreBootstrap.Controllers
{    
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
		private readonly IOptions<IdentityCookieOptions> _identityCookieOptions;

		public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IOptions<IdentityCookieOptions> identityCookieOptions)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _identityCookieOptions = identityCookieOptions;
        }

        [AllowAnonymous]
        [HttpGet("Register")]
        public IActionResult Register() => View();

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserViewModel userViewModel)
        {
            if(ModelState.IsValid)
            {      
                var user = new User { UserName = userViewModel.UserName, Email = userViewModel.Email, IsExternal = false };
                var result = await UserManager.CreateAsync(user, userViewModel.Password);
                if(result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, true);
                    return RedirectToAction("Users", "UserManagement");
                }
                else foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            }
            return View(userViewModel);
        }

        [AllowAnonymous]
        [HttpGet("Login")]
        public async Task<IActionResult> Login()
        {
            await HttpContext.Authentication.SignOutAsync(IdentityCookieOptions.Value.ExternalCookieAuthenticationScheme);
            return View();
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if(ModelState.IsValid)
            {
                var result = await SignInManager.PasswordSignInAsync(loginViewModel.UserName, loginViewModel.Password, loginViewModel.RememberMe, false);
                if(result.Succeeded) return RedirectToAction("Users", "UserManagement");
                ModelState.AddModelError("", "Invalid login attempt.");
            }
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
            if(user != null && !user.IsExternal)
            {
                // Already exists a local user with the external login mail.
                throw new Exception($"This email {user.Email} is already registered.");
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await SignInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                return RedirectToAction("Users", "UserManagement");
            }
            else if(result.IsLockedOut || result.IsNotAllowed || result.RequiresTwoFactor) throw new Exception(result.ToString());
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                if(await ConfirmExternalLogin(new UserManagementViewModel { Email = info.Principal.FindFirstValue(ClaimTypes.Email)}))
                {
                    return RedirectToAction("Users", "UserManagement");
                }
                else return RedirectToAction("Login");
            }
        }

        private async Task<bool> ConfirmExternalLogin(UserManagementViewModel viewModel)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if(info == null) return false;
            var user = new User { UserName = viewModel.Email, Email = viewModel.Email, IsExternal = true };
            var result = await UserManager.CreateAsync(user);
            if(result.Succeeded)
            {
                result = await UserManager.AddLoginAsync(user, info);
                if(result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false);
                    return true;
                }
            }
            return false;
        }

        [HttpGet("Edit"), Authorize]
        public async Task<IActionResult> Edit()
        {
            var user = await UserManager.GetUserAsync(HttpContext.User);
            if(user.IsExternal) return RedirectToAction("AccessDenied");
            var userViewModel = new UserManagementViewModel { UserId = user.Id };
            return View(userViewModel);
        }

        [HttpPost("Edit"), Authorize]
        public async Task<IActionResult> Edit(UserManagementViewModel viewModel)
        {
            var user = await UserManager.GetUserAsync(HttpContext.User);
            if(ModelState.IsValid)
            {
                var result = await UserManager.ChangePasswordAsync(user, viewModel.Password, viewModel.NewPassword);
                if(result.Succeeded) return RedirectToAction("Users", "UserManagement");
                foreach(var error in result.Errors) ModelState.AddModelError(error.Code, error.Description);
            }
            return View(viewModel);
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet("AccessDenied")]
        public IActionResult AccessDenied() => View();

        public SignInManager<User> SignInManager
        {
            get { return _signInManager; }
        }

        public UserManager<User> UserManager
        {
            get { return _userManager; }
        }

        public IOptions<IdentityCookieOptions> IdentityCookieOptions
        {
            get { return _identityCookieOptions; }
        }
    }
}
