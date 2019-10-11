using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace NetCoreBootstrap.Services.Intefaces
{
    public interface IAccountHelper
    {
        void SendConfirmationEmail(string userId, string userEmail, string token, string action);
        void SendRecoveryPasswordEmail(string userEmail, string token, string action);
        void SendNewPasswordEmail(string userEmail, string newPassword);
        string GenerateJwtToken(string userId, string userEmail);
        string GetUsernameFromRequest(HttpRequest request);
        string GenerateRandomPassword(int length = 8);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
