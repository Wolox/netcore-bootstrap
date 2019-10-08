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
    public class AccountController : BaseApiController
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly AccountHelper _accountHelper;
        private readonly IHtmlLocalizer<AccountController> _localizer;

        public AccountController(UserManager<User> userManager,
                                    SignInManager<User> signInManager,
                                    IUnitOfWork unitOfWork,
                                    IHtmlLocalizer<AccountController> localizer,
                                    IConfiguration configuration,
                                    IMailer mailer)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._unitOfWork = unitOfWork;
            this._localizer = localizer;
            this._accountHelper = new AccountHelper(configuration, this._localizer, mailer);
        }

        public UserManager<User> UserManager => this._userManager;
        public SignInManager<User> SignInManager => this._signInManager;
        public IUnitOfWork UnitOfWork => this._unitOfWork;
        public AccountHelper AccountHelper => this._accountHelper;
        public IHtmlLocalizer<AccountController> Localizer => this._localizer;

        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp([FromBody] UserSignUpVO userVO)
        {
            var user = new User
            {
                UserName = userVO.Email.ToLower(),
                Email = userVO.Email.ToLower(),
            };
            object response;
            try
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
            return Json(response);
        }

        [HttpPost("SignIn")]
        public async Task<IActionResult> SignIn([FromBody] UserSignInVO userVO)
        {
            object response;
            try
            {
                userVO.Email = userVO.Email.ToLower();
                var result = await SignInManager.PasswordSignInAsync(userVO.Email, userVO.Password, false, false);
                if (result.Succeeded)
                {
                    var user = UserManager.Users.Single(r => r.Email == userVO.Email);
                    Response.StatusCode = StatusCodes.Status200OK;
                    response = new UserVO(user.Email, $"Bearer {AccountHelper.GenerateJwtToken(user.Id, user.Email)}");
                }
                else if (result.IsNotAllowed)
                {
                    Response.StatusCode = StatusCodes.Status401Unauthorized;
                    response = new { Message = Localizer["account_login_confirm_email"].Value };
                }
                else
                    response = new { Message = Localizer["account_login_failed"].Value };
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
    }
}
