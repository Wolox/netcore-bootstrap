using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NetCoreBootstrap.Services.Intefaces;

namespace NetCoreBootstrap.Services.Helpers
{
    public class AccountHelper
    {
        private readonly string _authorizationKey = "Authorization";
        private readonly string _usernameKey = "cognito:username";
        private readonly IConfiguration _configuration;
        private readonly IHtmlLocalizer _localizer;
        private readonly IMailer _mailer;

        public AccountHelper(IConfiguration configuration, IHtmlLocalizer localizer, IMailer mailer)
        {
            this._configuration = configuration;
            this._localizer = localizer;
            this._mailer = mailer;
        }

        public IConfiguration Configuration => _configuration;
        public IHtmlLocalizer Localizer => _localizer;
        public IMailer Mailer => _mailer;

        public void SendConfirmationEmail(string userId, string userEmail, string token, string action)
        {
            var tokenHtml = HttpUtility.UrlEncode(token);
            var callbackUrl = $"{Configuration["AppUrl"]}{action}/{tokenHtml}";
            var subject = Localizer["account_email_subject"].Value;
            var body = Localizer["account_email_body"].Value + $" <a href='http://{callbackUrl}'>here.</a>";
            Mailer.SendMail(userEmail, subject, body);
        }

        public string GenerateJwtToken(string userId, string userEmail)
        {
            var claims = new ClaimsIdentity(new GenericIdentity(userEmail, "Token"), new[] { new Claim("ID", userId.ToString()) });
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(Configuration["Jwt:ExpireDays"]));
            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = Configuration["Jwt:Issuer"],
                Audience = Configuration["Jwt:Issuer"],
                SigningCredentials = creds,
                Subject = claims,
                Expires = expires,
            });
            return handler.WriteToken(securityToken);
        }

        public string GetUsernameFromRequest(HttpRequest request)
        {
            var decodedToken = GetDecodedToken(request);
            return decodedToken.Payload[_usernameKey].ToString();
        }

        private JwtSecurityToken GetDecodedToken(HttpRequest request)
        {
            var encodedToken = request.Headers[_authorizationKey].ToString().Split(" ").Last();
            return new JwtSecurityTokenHandler().ReadToken(encodedToken) as JwtSecurityToken;
        }
    }
}
