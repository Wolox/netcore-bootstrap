using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
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
    public class AccountHelper : IAccountHelper
    {
        private readonly string _authorizationKey = "Authorization";
        private readonly string _usernameKey = "unique_name";
        private readonly IConfiguration _configuration;
        private readonly IHtmlLocalizer<AccountHelper> _localizer;
        private readonly IMailer _mailer;

        public AccountHelper(IConfiguration configuration, IMailer mailer, IHtmlLocalizer<AccountHelper> localizer)
        {
            this._configuration = configuration;
            this._localizer = localizer;
            this._mailer = mailer;
        }

        public IConfiguration Configuration => _configuration;
        public IHtmlLocalizer<AccountHelper> Localizer => _localizer;
        public IMailer Mailer => _mailer;

        public void SendConfirmationEmail(string userId, string userEmail, string token, string action)
        {
            var tokenHtml = HttpUtility.UrlEncode(token);
            var callbackUrl = $"{Configuration["AppUrl"]}{action}/{tokenHtml}";
            var subject = Localizer["AccountEmailConfirmationSubject"].Value;
            var body = Localizer["AccountEmailConfirmationBody"].Value + $" <a href='http://{callbackUrl}'>here.</a>";
            Mailer.SendMail(userEmail, subject, body);
        }

        public void SendRecoveryPasswordEmail(string userEmail, string token, string action)
        {
            var tokenHtml = HttpUtility.UrlEncode(token);
            var callbackUrl = $"{Configuration["AppUrl"]}{action}/{tokenHtml}";
            var subject = Localizer["AccountForgotPasswordEmailSubject"].Value;
            var body = Localizer["AccountForgotPasswordEmailBody"].Value + $" <a href='http://{callbackUrl}'>here.</a>";
            Mailer.SendMail(userEmail, subject, body);
        }

        public void SendNewPasswordEmail(string userEmail, string newPassword)
        {
            var subject = Localizer["AccountNewPasswordEmailSubject"].Value;
            var body = Localizer["AccountNewPasswordEmailBody"].Value + $": {newPassword}";
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
            => GetDecodedToken(request).Payload[_usernameKey].ToString();

        public string GenerateRandomPassword(int length = 8)
        {
            if (length < 6) throw new ArgumentException();
            string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!$+<[%,=]>*+-/)(¡?¿·#¬|";
            StringBuilder result = new StringBuilder();
            result.Append(GetRandomValueFromSequence(characters, 0, characters.IndexOf("A") - 1));
            result.Append(GetRandomValueFromSequence(characters, characters.IndexOf("A"), characters.IndexOf("0") - 1));
            result.Append(GetRandomValueFromSequence(characters, characters.IndexOf("0"), characters.IndexOf("!") - 1));
            result.Append(GetRandomValueFromSequence(characters, characters.IndexOf("!"), characters.Length));
            while (result.Length < length)
            {
                result.Append(GetRandomValueFromSequence(characters, 0, characters.Length));
            }
            return result.ToString();
        }

        private static string GetRandomValueFromSequence(string sequence, int minIndex, int maxIndex)
        {
            var random = new Random();
            return sequence[random.Next(minIndex, maxIndex)].ToString();
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                                .GetBytes(Configuration["SignInKey"])),
                ValidateLifetime = false
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");
            return principal;
        }

        private JwtSecurityToken GetDecodedToken(HttpRequest request)
        {
            var encodedToken = request.Headers[_authorizationKey].ToString().Split(" ").Last();
            return new JwtSecurityTokenHandler().ReadToken(encodedToken) as JwtSecurityToken;
        }
    }
}
