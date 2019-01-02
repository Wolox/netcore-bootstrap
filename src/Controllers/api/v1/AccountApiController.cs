using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NetCoreBootstrap.Helpers;
using NetCoreBootstrap.Mail;
using NetCoreBootstrap.Models.Database;
using NetCoreBootstrap.Models.Views;
using NetCoreBootstrap.Models.VOs;
using NetCoreBootstrap.Repositories.Interfaces;

namespace NetCoreBootstrap.Api.V1.Controllers
{
    [Route("/api/v1/[controller]/[action]")]
    public class AccountApiController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IMailer _mailer;
        private readonly IHtmlLocalizer<AccountApiController> _localizer;
        private readonly IUnitOfWork _unitOfWork;

        public AccountApiController(UserManager<User> userManager,
                                    SignInManager<User> signInManager,
                                    IUnitOfWork unitOfWork,
                                    IConfiguration configuration,
                                    IMailer mailer,
                                    IHtmlLocalizer<AccountApiController> localizer)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._configuration = configuration;
            this._mailer = mailer;
            this._localizer = localizer;
            this._unitOfWork = unitOfWork;
        }

        public UserManager<User> UserManager { get => this._userManager; }
        public SignInManager<User> SignInManager { get => this._signInManager; }
        public IConfiguration Configuration { get => this._configuration; }
        public IMailer Mailer { get => this._mailer; }
        public IHtmlLocalizer<AccountApiController> Localizer { get => this._localizer; }
        public IUnitOfWork UnitOfWork { get => this._unitOfWork; }

        [HttpPost("SignIn")]
        public async Task<object> SignIn([FromBody] UserVO userValueObject)
        {
            object response;
            try
            {
                if (!string.IsNullOrEmpty(userValueObject.Email)) userValueObject.Email = userValueObject.Email.ToLower();
                var result = await SignInManager.PasswordSignInAsync(userValueObject.Email, userValueObject.Password, userValueObject.RememberMe, false);
                if (result.Succeeded)
                {
                    var appUser = UserManager.Users.SingleOrDefault(r => r.Email == userValueObject.Email);
                    Response.StatusCode = StatusCodes.Status200OK;
                    var configVariables = new Dictionary<string, string>
                    {
                        { "key", Configuration["Jwt:Key"] },
                        { "expire", Configuration["Jwt:ExpireDays"] },
                        { "issuer", Configuration["Jwt:Issuer"] },
                    };
                    response = new UserVO
                    {
                        Token = $"Bearer {AccountHelper.GenerateJwtToken(userValueObject.Email, appUser, configVariables)}",
                        Email = appUser.Email,
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
                    if (string.IsNullOrEmpty(userValueObject.Email) || string.IsNullOrEmpty(userValueObject.Password))
                        response = new { Message = Localizer["account_login_failed_empty_fields"].Value };
                    else
                        response = new { Message = Localizer["account_login_failed"].Value };
                }
                return Json(response);
            }
            catch (Exception e)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return Json(new { Message = e.Message });
            }
        }

        [HttpPost("ExternalSignIn")]
        public async Task<object> ExternalSignIn([FromBody] UserVO userValueObject)
        {
            object response;
            string provider = userValueObject.IsFacebook ? "Facebook" : "Google";
            IdentityResult result = null;
            User user = null;
            if (!UserManager.Users.Any(u => u.Email == userValueObject.Email))
            {
                result = await UserManager.CreateAsync(new User
                {
                    Email = userValueObject.Email,
                    UserName = userValueObject.Email,
                    EmailConfirmed = true,
                    IsExternal = true,
                });
                user = await UserManager.FindByEmailAsync(userValueObject.Email);
                await UserManager.AddLoginAsync(user, new UserLoginInfo(provider, userValueObject.UserId, user.Email));
            }
            if (result == null || result.Succeeded)
            {
                if (user == null) user = await UserManager.FindByEmailAsync(userValueObject.Email);
                var signInResult = await SignInManager.ExternalLoginSignInAsync(provider, userValueObject.UserId, false);
                if (signInResult.Succeeded)
                {
                    Response.StatusCode = StatusCodes.Status200OK;
                    var configVariables = new Dictionary<string, string>
                    {
                        { "key", Configuration["Jwt:Key"] },
                        { "expire", Configuration["Jwt:ExpireDays"] },
                        { "issuer", Configuration["Jwt:Issuer"] },
                    };
                    response = new { Token = $"Bearer {AccountHelper.GenerateJwtToken(userValueObject.Email, user, configVariables)}" };
                }
                else
                {
                    string message = Localizer["account_external_login_failed"].Value;
                    var logins = await UserManager.GetLoginsAsync(user);
                    if (!logins.Any(login => login.LoginProvider == provider))
                        message = Localizer["account_external_login_invalid_provider"].Value + $"{provider}";
                    else if (!logins.Any(login => login.ProviderKey == userValueObject.UserId))
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

        [HttpPost("SignUp")]
        public async Task<object> SignUp([FromBody] UserVO userValueObject)
        {
            var user = new User
            {
                UserName = userValueObject.Email.ToLower(),
                Email = userValueObject.Email.ToLower(),
            };
            object response;
            if (!user.IsEmailValid())
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new { Message = Localizer["account_user_not_created_invalid_email"].Value };
            }
            else if (userValueObject.Password != userValueObject.ConfirmPassword)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new { Message = Localizer["account_user_not_created_password_did_not_match"].Value };
            }
            else
            {
                try
                {
                    var result = await UserManager.CreateAsync(user, userValueObject.Password);
                    if (result.Succeeded)
                    {
                        await SendConfirmationEmailAsync(user);
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

        [HttpGet("ConfirmEmail/{userId}/{token}")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = UserManager.FindByIdAsync(userId).Result;
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
        public async Task<IActionResult> ForgotPassword([FromBody] UserVO userValueObject)
        {
            object response;
            var user = await _userManager.FindByEmailAsync(userValueObject.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new { Message = Localizer["account_forgot_password_user_not_found"].Value };
            }
            else
            {
                await SendRecoveryPasswordEmailAsync(user);
                Response.StatusCode = StatusCodes.Status200OK;
                response = new { Message = Localizer["account_forgot_password_email_sent"].Value };
            }
            return Json(response);
        }

        [HttpPost("EditAccount")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> EditAccount([FromBody] UserVO userValueObject)
        {
            var user = await UserManager.FindByEmailAsync(User.Identity.Name);
            object response;
            if (user == null || !User.Identity.IsAuthenticated)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new { Message = Localizer["account_user_not_authenticated"].Value };
            }
            else if (userValueObject.NewPassword != userValueObject.ConfirmPassword)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new { Message = Localizer["account_user_not_created_password_did_not_match"].Value };
            }
            else
            {
                var result = await UserManager.ChangePasswordAsync(user, userValueObject.Password, userValueObject.NewPassword);
                if (result.Succeeded)
                {
                    Response.StatusCode = StatusCodes.Status200OK;
                    response = new { Message = Localizer["account_password_changed"].Value };
                }
                else
                {
                    Response.StatusCode = StatusCodes.Status400BadRequest;
                    response = new { Message = $"{Localizer["account_password_could_not_be_changed"].Value}: {string.Join(" ", result.Errors.Select(err => err.Description))}" };
                }
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
                SendNewPasswordEmailAsync(user, newPassword);
                Response.StatusCode = StatusCodes.Status200OK;
                response = new { Message = Localizer["account_new_password_confirmed"].Value };
            }
            else
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new { Message = Localizer["account_new_password_not_confirmed"].Value, Errors = result.Errors };
            }
            return Json(response);
        }

        private void SendNewPasswordEmailAsync(User user, string newPassword)
        {
            var subject = Localizer["account_new_password_email_subject"].Value;
            var body = Localizer["account_new_password_email_body"].Value + $": {newPassword}";
            Mailer.SendMail(user.Email, subject, body);
        }

        private async Task SendConfirmationEmailAsync(User user)
        {
            var token = await UserManager.GenerateEmailConfirmationTokenAsync(user);
            var tokenHtml = HttpUtility.UrlEncode(token);
            var callbackUrl = Configuration["AppUrl"] + this.Url.Action("ConfirmEmail", "AccountApi", new { userId = user.Id, token = tokenHtml });
            var subject = Localizer["account_email_subject"].Value;
            var body = Localizer["account_email_body"].Value + $" <a href='http://{callbackUrl}'>here.</a>";
            Mailer.SendMail(user.Email, subject, body);
        }

        private async Task SendRecoveryPasswordEmailAsync(User user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var tokenHtml = HttpUtility.UrlEncode(token);
            var callbackUrl = Configuration["AppUrl"] + this.Url.Action("ResetPassword", "AccountApi", new { userId = user.Id, token = tokenHtml });
            var subject = Localizer["account_forgot_password_email_subject"].Value;
            var body = Localizer["account_forgot_password_email_body"].Value + $" <a href='http://{callbackUrl}'>here.</a>";
            Mailer.SendMail(user.Email, subject, body);
        }
    }
}
