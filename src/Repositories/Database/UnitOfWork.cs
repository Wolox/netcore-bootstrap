using Microsoft.AspNetCore.Identity;
using NetCoreBootstrap.Models.Database;
using NetCoreBootstrap.Repositories.Interfaces;

namespace NetCoreBootstrap.Repositories.Database
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataBaseContext _context;

        public UnitOfWork(DataBaseContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            this._context = context;
            this.UserRepository = new UserRepository(context, userManager, roleManager);
        }

        public IUserRepository UserRepository { get; private set; }

        public int Complete()
        {
            return this._context.SaveChanges();
        }

        public void Dispose()
        {
            this._context.Dispose();
        }
    }
}
