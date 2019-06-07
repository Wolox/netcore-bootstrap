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
    }
}
