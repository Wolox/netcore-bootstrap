using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Configuration;
using NetCoreBootstrap.Core.Models.Database;
using NetCoreBootstrap.Core.Models.VOs;
using NetCoreBootstrap.Data.Repositories.Interfaces;
using NetCoreBootstrap.Services.Helpers;
using NetCoreBootstrap.Services.Intefaces;

namespace NetCoreBootstrap.Api.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]")]
    public class AccountApiController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly AccountHelper _accountHelper;
        private readonly IHtmlLocalizer<AccountApiController> _localizer;

        public AccountApiController(UserManager<User> userManager,
                                    SignInManager<User> signInManager,
                                    IUnitOfWork unitOfWork,
                                    IHtmlLocalizer<AccountApiController> localizer,
                                    IConfiguration configuration)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._unitOfWork = unitOfWork;
            this._localizer = localizer;
            this._accountHelper = new AccountHelper(configuration, this._localizer);
        }

        public UserManager<User> UserManager { get => this._userManager; }
        public SignInManager<User> SignInManager { get => this._signInManager; }
        public IUnitOfWork UnitOfWork { get => this._unitOfWork; }
        public AccountHelper AccountHelper { get => this._accountHelper; }
        public IHtmlLocalizer<AccountApiController> Localizer { get => this._localizer; }

        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp([FromBody] UserSignUpVO userVO)
        {
            var user = new User
            {
                UserName = userVO.Email.ToLower(),
                Email = userVO.Email.ToLower(),
            };
            object response;
            if (!user.IsEmailValid())
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new { Message = Localizer["account_user_not_created_invalid_email"].Value };
            }
            else if (userVO.Password != userVO.ConfirmPassword)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new { Message = Localizer["account_user_not_created_password_did_not_match"].Value };
            }
            else
            {
                try
                {
                    var result = await UserManager.CreateAsync(user, userVO.Password);
                    if (result.Succeeded)
                    {
                        var token = await UserManager.GenerateEmailConfirmationTokenAsync(user);
                        AccountHelper.SendConfirmationEmail(user.Id, user.Email, token, Url.Action("ConfirmEmail", "AccountApi", new { userId = user.Id }));
                        Response.StatusCode = StatusCodes.Status200OK;
                        response = new { Message = Localizer["account_user_created"].Value };
                    }
                    else
                    {
                        Response.StatusCode = StatusCodes.Status400BadRequest;
                        response = new { Message = $"{Localizer["account_user_not_created"].Value}{result.Errors.Select(e => e.Description).Last()}" };
                    }
                }
                catch (ArgumentNullException e)
                {
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    response = new { Message = $"{Localizer["account_user_not_created"].Value}{e.Message}" };
                }
            }
            return Json(response);
        }

        [HttpPost("SignIn")]
        public async Task<IActionResult> SignIn([FromBody] UserSignInVO userVO)
        {
            object response;
            try
            {
                if (!string.IsNullOrEmpty(userVO.Email)) userVO.Email = userVO.Email.ToLower();
                var result = await SignInManager.PasswordSignInAsync(userVO.Email, userVO.Password, false, false);
                if (result.Succeeded)
                {
                    var user = UserManager.Users.Single(r => r.Email == userVO.Email);
                    Response.StatusCode = StatusCodes.Status200OK;
                    response = new UserVO
                    {
                        Token = $"Bearer {AccountHelper.GenerateJwtToken(user.Id, user.Email)}",
                        Email = user.Email,
                    };
                }
                else if (result.IsNotAllowed)
                {
                    Response.StatusCode = StatusCodes.Status401Unauthorized;
                    response = new { Message = Localizer["account_login_confirm_email"].Value };
                }
                else
                {
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    if (string.IsNullOrEmpty(userVO.Email) || string.IsNullOrEmpty(userVO.Password))
                        response = new { Message = Localizer["account_login_failed_empty_fields"].Value };
                    else
                        response = new { Message = Localizer["account_login_failed"].Value };
                }
                return Json(response);
            }
            catch (InvalidOperationException)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Json(new { Message = Localizer["UserEmailDoesNotExists"].Value });
            }
            catch (Exception e)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Json(new { Message = e.Message });
            }
        }

        [HttpPost("ExternalSignUp")]
        public async Task<IActionResult> ExternalSignIn([FromBody] UserSignUpVO userVO)
        {
            object response;
            string provider = userVO.IsFacebook ? "Facebook" : "Google";
            IdentityResult result = null;
            User user = null;
            if (!UserManager.Users.Any(u => u.Email == userVO.Email))
            {
                result = await UserManager.CreateAsync(new User
                {
                    Email = userVO.Email,
                    UserName = userVO.Email,
                    EmailConfirmed = true,
                    IsExternal = true,
                });
                user = await UserManager.FindByEmailAsync(userVO.Email);
                await UserManager.AddLoginAsync(user, new UserLoginInfo(provider, userVO.ExternalUserId, user.Email));
            }
            if (result == null || result.Succeeded)
            {
                if (user == null) user = await UserManager.FindByEmailAsync(userVO.Email);
                var signInResult = await SignInManager.ExternalLoginSignInAsync(provider, userVO.ExternalUserId, false);
                if (signInResult.Succeeded)
                {
                    Response.StatusCode = StatusCodes.Status200OK;
                    response = new { Token = $"Bearer {AccountHelper.GenerateJwtToken(user.Id, user.Email)}" };
                }
                else
                {
                    string message = Localizer["account_external_login_failed"].Value;
                    var logins = await UserManager.GetLoginsAsync(user);
                    if (!logins.Any(login => login.LoginProvider == provider))
                        message = Localizer["account_external_login_invalid_provider"].Value + $"{provider}";
                    else if (!logins.Any(login => login.ProviderKey == userVO.ExternalUserId))
                        message = Localizer["account_external_login_invalid_user_id"].Value;
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    response = new { Message = message, Errors = string.Join(";", signInResult.ToString()) };
                }
            }
            else
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new { Message = Localizer["account_external_login_failed"].Value, Errors = string.Join(";", result.Errors) };
            }
            return Json(response);
        }

        [HttpGet("ConfirmEmail/{userId}/{token}")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await UserManager.FindByIdAsync(userId);
            var result = await UserManager.ConfirmEmailAsync(user, HttpUtility.UrlDecode(token));
            object response;
            if (result.Succeeded)
            {
                Response.StatusCode = StatusCodes.Status200OK;
                response = new { Message = Localizer["account_email_confirmed"].Value };
            }
            else
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new { Message = Localizer["account_email_not_confirmed"].Value, Errors = result.Errors };
            }
            return Json(response);
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
