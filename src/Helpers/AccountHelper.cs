using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using NetCoreBootstrap.Models.Database;
using Microsoft.IdentityModel.Tokens;

namespace NetCoreBootstrap.Helpers
{
    public static class AccountHelper
    {
        public static string GenerateRandomPassword(int length = 8)
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

        public static string GenerateJwtToken(string email, User user, Dictionary<string, string> configVariables)
        {
            var claims = new ClaimsIdentity(new GenericIdentity(user.Email, "Token"), new[] { new Claim("ID", user.Id.ToString()) });
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configVariables["key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(configVariables["expire"]));
            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = configVariables["issuer"],
                Audience = configVariables["issuer"],
                SigningCredentials = creds,
                Subject = claims,
                Expires = expires,
            });
            return handler.WriteToken(securityToken);
        }

        private static string GetRandomValueFromSequence(string sequence, int minIndex, int maxIndex)
        {
            var random = new Random();
            return sequence[random.Next(minIndex, maxIndex)].ToString();
        }
    }
}
