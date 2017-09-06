using Microsoft.AspNetCore.Mvc;
using NetCoreBootstrap.Models.Database;
using NetCoreBootstrap.Repositories;
using Microsoft.EntityFrameworkCore;
using NetCoreBootstrap.Models.Views;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Threading.Tasks;

namespace NetCoreBootstrap.Controllers
{
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet("Register")]
        public IActionResult Register() => View();

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserViewModel userViewModel)
        {
            if(ModelState.IsValid)
            {      
                var user = new User { UserName = userViewModel.UserName, Email = userViewModel.Email };
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

        [HttpGet("Login")]
        public IActionResult Login() => View();

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if(ModelState.IsValid)
            {
                var result = await SignInManager.PasswordSignInAsync(loginViewModel.UserName, loginViewModel.Password, loginViewModel.RememberMe, false);
                if(result.Succeeded)
                {
                    return RedirectToAction("Users", "UserManagement");
                }
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(loginViewModel);
            }
            return View(loginViewModel);
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
    }
}
