using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using NetCoreBootstrap.Core.Models.Database;
using NetCoreBootstrap.Data.Repositories.Database;
using NetCoreBootstrap.Data.Repositories.Interfaces;

namespace NetCoreBootstrap.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DatabaseContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRepository(DatabaseContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            this._context = context;
            this._userManager = userManager;
            this._roleManager = roleManager;
        }

        public UserManager<User> UserManager { get => this._userManager; }
        public RoleManager<IdentityRole> RoleManager { get => this._roleManager; }
        public DatabaseContext Context { get => this._context; }

        public User GetByUsername(string username) => Context.Users.Single(u => u.Email == username);

        public void SaveRefreshToken(User user, string token)
        {
            var refreshToken = new RefreshToken
            {
                Token = token,
                ValidFrom = DateTime.Now,
                UserId = user.Id,
                User = user,
                ValidTo = DateTime.Now.AddDays(30),
            };
            Context.RefreshTokens.Add(refreshToken);
        }

        public IEnumerable<string> GetRefreshToken(User user) =>
            Context.RefreshTokens.Where(rt => rt.UserId == user.Id).Select(rt => rt.Token);

        public void DeleteRefreshToken(User user, string refreshToken)
        {
            var token = Context.RefreshTokens.First(rt => rt.Token == refreshToken);
            Context.RefreshTokens.Remove(token);
        }
    }
}
