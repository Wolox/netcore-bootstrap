
using Microsoft.AspNetCore.Identity;
using NetCoreBootstrap.Core.Models.Database;
using NetCoreBootstrap.Data.Repositories.Interfaces;

namespace NetCoreBootstrap.Data.Repositories.Database
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DatabaseContext _context;

        public UnitOfWork(DatabaseContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
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