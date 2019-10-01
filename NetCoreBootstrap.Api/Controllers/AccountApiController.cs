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
                response = new { Message = Localizer["AccountUserNotCreatedInvalidEmail"].Value };
            }
            else if (userVO.Password != userVO.ConfirmPassword)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new { Message = Localizer["AccountUserNotCreatedPasswordDidNotMatch"].Value };
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
                        response = new { Message = Localizer["AccountUserCreated"].Value };
                    }
                    else
                    {
                        Response.StatusCode = StatusCodes.Status400BadRequest;
                        response = new { Message = $"{Localizer["AccountUserNotCreated"].Value}{result.Errors.Select(e => e.Description).Last()}" };
                    }
                }
                catch (ArgumentNullException e)
                {
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    response = new { Message = $"{Localizer["AccountUserNotCreated"].Value}{e.Message}" };
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
                    var refreshToken = AccountHelper.GenerateRefreshToken();
                    UnitOfWork.UserRepository.SaveRefreshToken(user, refreshToken);
                    Response.StatusCode = StatusCodes.Status200OK;
                    response = new UserVO
                    {
                        Token = $"Bearer {AccountHelper.GenerateJwtToken(user.Id, user.Email)}",
                        Email = user.Email,
                        RefreshToken = refreshToken,
                    };
                }
                else if (result.IsNotAllowed)
                {
                    Response.StatusCode = StatusCodes.Status401Unauthorized;
                    response = new { Message = Localizer["AccountLoginConfirmEmail"].Value };
                }
                else
                {
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    if (string.IsNullOrEmpty(userVO.Email) || string.IsNullOrEmpty(userVO.Password))
                        response = new { Message = Localizer["AccountLoginFailedEmptyFields"].Value };
                    else
                        response = new { Message = Localizer["AccountLoginFailed"].Value };
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
                    string message = Localizer["AccountExternalLoginFailed"].Value;
                    var logins = await UserManager.GetLoginsAsync(user);
                    if (!logins.Any(login => login.LoginProvider == provider))
                        message = Localizer["AccountExternalLoginInvalidProvider"].Value + $"{provider}";
                    else if (!logins.Any(login => login.ProviderKey == userVO.ExternalUserId))
                        message = Localizer["AccountExternalLoginInvalidUserId"].Value;
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    response = new { Message = message, Errors = string.Join(";", signInResult.ToString()) };
                }
            }
            else
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new { Message = Localizer["AccountExternalLoginFailed"].Value, Errors = string.Join(";", result.Errors) };
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
                response = new { Message = Localizer["AccountEmailConfirmed"].Value };
            }
            else
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new { Message = Localizer["AccountEmailNotConfirmed"].Value, Errors = result.Errors };
            }
            return Json(response);
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] UserSignInVO userVO)
        {
            object response;
            var user = await UserManager.FindByEmailAsync(userVO.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new { Message = Localizer["AccountForgotPasswordUserNotFound"].Value };
            }
            else
            {
                var token = await UserManager.GeneratePasswordResetTokenAsync(user);
                AccountHelper.SendRecoveryPasswordEmail(user.Email, token, Url.Action("ResetPassword", "AccountApi", new { userId = user.Id }));
                Response.StatusCode = StatusCodes.Status200OK;
                response = new { Message = Localizer["AccountForgotPasswordEmailSent"].Value };
            }
            return Json(response);
        }

        [HttpGet("ResetPassword/{userId}/{token}")]
        public async Task<IActionResult> ResetPassword(string userId, string token)
        {
            var user = await UserManager.FindByIdAsync(userId);
            var newPassword = AccountHelper.GenerateRandomPassword(8);
            var result = await UserManager.ResetPasswordAsync(user, HttpUtility.UrlDecode(token), newPassword);
            object response;
            if (result.Succeeded)
            {
                AccountHelper.SendNewPasswordEmail(user.Email, newPassword);
                Response.StatusCode = StatusCodes.Status200OK;
                response = new { Message = Localizer["AccountNewPasswordConfirmed"].Value };
            }
            else
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new { Message = Localizer["AccountNewPasswordNotConfirmed"].Value, Errors = result.Errors };
            }
            return Json(response);
        }

        [HttpPost("RefreshToken")]
        public IActionResult Refresh([FromBody] RefreshTokenVO refreshTokenVO)
        {       
            object response;            
            var principal = AccountHelper.GetPrincipalFromExpiredToken(refreshTokenVO.Token);
            var user = UnitOfWork.UserRepository.GetByUsername(principal.Identity.Name);
            var savedRefreshToken = UnitOfWork.UserRepository.GetRefreshToken(user);
            if (!savedRefreshToken.Any(rt => rt == refreshTokenVO.RefreshToken))
            {
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                response = new { Message = Localizer["AccountInvalidRefreshToken"].Value };
            }
            else
            {
                var newToken = AccountHelper.GenerateJwtToken(user.Id, user.Email);
                var newRefreshToken = AccountHelper.GenerateRefreshToken();
                UnitOfWork.UserRepository.DeleteRefreshToken(user, refreshTokenVO.RefreshToken);
                UnitOfWork.UserRepository.SaveRefreshToken(user, newRefreshToken);
                UnitOfWork.Complete();
                response = new UserVO
                {
                    Token = $"Bearer {newToken}",
                    RefreshToken = newRefreshToken,
                    Email = user.Email,
                };
            }
            return Json(response);
        }
    }
}
