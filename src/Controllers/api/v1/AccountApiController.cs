using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NetCoreBootstrap.Models.Database;
using NetCoreBootstrap.Models.Views;

namespace NetCoreBootstrap.Api.V1.Controllers
{
    [Route("/api/v1/[controller]/[action]")]
    public class AccountApiController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public AccountApiController(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._configuration = configuration;
        }

        public UserManager<User> UserManager
        {
            get { return this._userManager; }
        }

        public SignInManager<User> SignInManager
        {
            get { return this._signInManager; }
        }

        public IConfiguration Configuration
        {
            get { return this._configuration; }
        }

        [HttpPost]
        public async Task<object> Login([FromBody] LoginViewModel loginViewModel)
        {
            var result = await SignInManager.PasswordSignInAsync(loginViewModel.UserName, loginViewModel.Password, loginViewModel.RememberMe, false);
            if (result.Succeeded)
            {
                var appUser = UserManager.Users.SingleOrDefault(r => r.Email == loginViewModel.UserName);
                return GenerateJwtToken(loginViewModel.UserName, appUser);
            }
            throw new ApplicationException("INVALID_LOGIN_ATTEMPT");
        }

        [HttpPost]
        public async Task<object> Register([FromBody] UserViewModel userViewModel)
        {
            var user = new User
            {
                UserName = userViewModel.Email,
                Email = userViewModel.Email,
            };
            var result = await UserManager.CreateAsync(user, userViewModel.Password);
            if (result.Succeeded)
            {
                await SignInManager.SignInAsync(user, false);
                return GenerateJwtToken(userViewModel.Email, user);
            }
            throw new ApplicationException("UNKNOWN_ERROR");
        }

        [Authorize]
        [HttpGet]
        public string Protected()
        {
            return "Protected area";
        }

        private string GenerateJwtToken(string email, User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(Configuration["JwtExpireDays"]));
            var token = new JwtSecurityToken(
                Configuration["JwtIssuer"],
                Configuration["JwtIssuer"],
                claims,
                expires: expires,
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
