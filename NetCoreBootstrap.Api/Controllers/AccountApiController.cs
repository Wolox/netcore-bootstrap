using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NetCoreBootstrap.Core.Models.Database;
using NetCoreBootstrap.Data.Repositories.Interfaces;

namespace NetCoreBootstrap.Api.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]")]
    public class AccountApiController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public AccountApiController(UserManager<User> userManager,
                                    SignInManager<User> signInManager,
                                    IUnitOfWork unitOfWork)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._unitOfWork = unitOfWork;
        }

        public UserManager<User> UserManager { get => this._userManager; }
        public SignInManager<User> SignInManager { get => this._signInManager; }
        public IUnitOfWork UnitOfWork { get => this._unitOfWork; }

        [HttpPost("SignUp")]
        public IActionResult SignUp()
        {
            return Ok(new {});
        }

        [HttpPost("SignIn")]
        public IActionResult SignIn()
        {
            return Ok(new {});
        }

        [HttpPost("ExternalSignUp")]
        public IActionResult ExternalSignIn()
        {
            return Ok(new {});
        }

        [HttpGet("ConfirmEmail/{userId}/{token}")]
        public IActionResult ConfirmEmail(string userId, string token)
        {
            return Ok(new {});
        }

        [HttpPost("ForgotPassword")]
        public IActionResult ForgotPassword()
        {
            return Ok(new {});
        }

        [HttpGet("ResetPassword/{userId}/{token}")]
        public IActionResult ResetPassword(string userId, string token)
        {
            return Ok(new {});
        }
    }
}
