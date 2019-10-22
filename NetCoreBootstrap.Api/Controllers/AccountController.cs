using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
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
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAccountHelper _accountHelper;
        private readonly IHtmlLocalizer<AccountController> _localizer;

        public AccountController(UserManager<User> userManager,
                                    SignInManager<User> signInManager,
                                    IUnitOfWork unitOfWork,
                                    IHtmlLocalizer<AccountController> localizer,
                                    IConfiguration configuration,
                                    IMailer mailer,
                                    IAccountHelper accountHelper)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._unitOfWork = unitOfWork;
            this._localizer = localizer;
            this._accountHelper = accountHelper;
        }

        public UserManager<User> UserManager => _userManager;
        public SignInManager<User> SignInManager => _signInManager;
        public IUnitOfWork UnitOfWork => _unitOfWork;
        public IAccountHelper AccountHelper => _accountHelper;
        public IHtmlLocalizer<AccountController> Localizer => _localizer;

        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp([FromBody] UserSignUpVO userVO)
        {
            var user = new User
            {
                UserName = userVO.Email.ToLower(),
                Email = userVO.Email.ToLower(),
            };
            IActionResult response;
            try
            {
                if (userVO.Password == userVO.ConfirmPassword)
                {
                    var result = await UserManager.CreateAsync(user, userVO.Password);
                    if (result.Succeeded)
                    {
                        var token = await UserManager.GenerateEmailConfirmationTokenAsync(user);
                        AccountHelper.SendConfirmationEmail(user.Id,
                                                            user.Email,
                                                            token,
                                                            Url.Action("ConfirmEmail", "AccountApi",
                                                            new { userId = user.Id }));
                        response = Ok(Localizer["AccountUserCreated"].Value);
                    }
                    else
                        response = BadRequest($"{Localizer["AccountUserNotCreated"].Value}{result.Errors.Select(e => e.Description).Last()}" );
                }
                else
                    response = BadRequest(Localizer["AccountUserNotCreatedPasswordDidNotMatch"]);
            }
            catch (ArgumentNullException e)
            {
                response = BadRequest($"{Localizer["AccountUserNotCreated"].Value}{e.Message}");
            }
            return response;
        }

        [HttpPost("SignIn")]
        public async Task<IActionResult> SignIn([FromBody] UserSignInVO userVO)
        {
            IActionResult response;
            try
            {
                userVO.Email = userVO.Email.ToLower();
                var result = await SignInManager.PasswordSignInAsync(userVO.Email, userVO.Password, false, false);
                if (result.Succeeded)
                {
                    var user = UserManager.Users.Single(r => r.Email == userVO.Email);
                    response = new JsonResult(new UserVO(user.Email, $"Bearer {AccountHelper.GenerateJwtToken(user.Id, user.Email)}"));
                }
                else if (result.IsNotAllowed)
                    response = BadRequest(Localizer["AccountLoginConfirmEmail"].Value);
                else
                    response = BadRequest(Localizer["AccountLoginFailed"].Value);
                return response;
            }
            catch (InvalidOperationException)
            {
                return new JsonResult(Localizer["UserEmailDoesNotExist"].Value);
            }
            catch (Exception e)
            {
                return new JsonResult(new { Message = e.Message });
            }
        }

        [HttpPost("ExternalSignIn")]
        public async Task<IActionResult> ExternalSignIn([FromBody] UserSignUpVO userVO)
        {
            IActionResult response;
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
                    response = new JsonResult(new { Token = $"Bearer {AccountHelper.GenerateJwtToken(user.Id, user.Email)}" });
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
                    response = new JsonResult(new { Message = message, Errors = string.Join(";", signInResult.ToString()) });
                }
            }
            else
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new JsonResult(new { Message = Localizer["AccountExternalLoginFailed"].Value, Errors = string.Join(";", result.Errors) });
            }
            return response;
        }

        [HttpGet("ConfirmEmail/{userId}/{token}")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await UserManager.FindByIdAsync(userId);
            var result = await UserManager.ConfirmEmailAsync(user, HttpUtility.UrlDecode(token));
            IActionResult response;
            if (result.Succeeded)
            {
                Response.StatusCode = StatusCodes.Status200OK;
                response = Ok(Localizer["AccountEmailConfirmed"].Value);
            }
            else
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new JsonResult(new { Message = Localizer["AccountEmailNotConfirmed"].Value, Errors = result.Errors });
            }
            return response;
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] UserSignInVO userVO)
        {
            IActionResult response;
            var user = await UserManager.FindByEmailAsync(userVO.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                response = BadRequest(Localizer["AccountForgotPasswordUserNotFound"].Value);
            else
            {
                var token = await UserManager.GeneratePasswordResetTokenAsync(user);
                AccountHelper.SendRecoveryPasswordEmail(user.Email, token, Url.Action("ResetPassword", "AccountApi", new { userId = user.Id }));
                Response.StatusCode = StatusCodes.Status200OK;
                response = Ok(Localizer["AccountForgotPasswordEmailSent"].Value);
            }
            return response;
        }

        [HttpGet("ResetPassword/{userId}/{token}")]
        public async Task<IActionResult> ResetPassword(string userId, string token)
        {
            var user = await UserManager.FindByIdAsync(userId);
            var newPassword = AccountHelper.GenerateRandomPassword(8);
            var result = await UserManager.ResetPasswordAsync(user, HttpUtility.UrlDecode(token), newPassword);
            IActionResult response;
            if (result.Succeeded)
            {
                AccountHelper.SendNewPasswordEmail(user.Email, newPassword);
                Response.StatusCode = StatusCodes.Status200OK;
                response = Ok(Localizer["AccountNewPasswordConfirmed"].Value);
            }
            else
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new JsonResult(new { Message = Localizer["AccountNewPasswordNotConfirmed"].Value, Errors = result.Errors });
            }
            return response;
        }

        [HttpPost("RefreshToken")]
        public IActionResult Refresh([FromBody] RefreshTokenVO refreshTokenVO)
        {       
            IActionResult response;            
            var principal = AccountHelper.GetPrincipalFromExpiredToken(refreshTokenVO.Token);
            var user = UnitOfWork.UserRepository.GetByUsername(principal.Identity.Name);
            var savedRefreshToken = UnitOfWork.UserRepository.GetRefreshToken(user);
            if (!savedRefreshToken.Any(rt => rt == refreshTokenVO.RefreshToken))
                response = Unauthorized(Localizer["AccountInvalidRefreshToken"].Value);
            else
            {
                var newToken = AccountHelper.GenerateJwtToken(user.Id, user.Email);
                var newRefreshToken = AccountHelper.GenerateRefreshToken();
                UnitOfWork.UserRepository.DeleteRefreshToken(user, refreshTokenVO.RefreshToken);
                UnitOfWork.UserRepository.SaveRefreshToken(user, newRefreshToken);
                UnitOfWork.Complete();
                response = new JsonResult(new UserVO
                {
                    Token = $"Bearer {newToken}",
                    RefreshToken = newRefreshToken,
                    Email = user.Email,
                });
            }
            return response;
        }
    }
}
