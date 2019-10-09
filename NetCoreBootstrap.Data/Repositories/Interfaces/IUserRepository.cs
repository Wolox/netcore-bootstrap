using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using NetCoreBootstrap.Core.Models.Database;

namespace NetCoreBootstrap.Data.Repositories.Interfaces
{
    public interface IUserRepository
    {
        void SaveRefreshToken(User user, string token);
        User GetByUsername(string username);
        IEnumerable<string> GetRefreshToken(User user);
        void DeleteRefreshToken(User user, string refreshToken);
    }
}
