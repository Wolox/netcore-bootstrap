using System.Linq;
using Microsoft.EntityFrameworkCore;
using MyMVCProject.Models.Database;

namespace MyMVCProject.Repositories
{
    public class UserRepository : MasterRepository<User>
    {
        public UserRepository(DbContextOptions<DataBaseContext> options) : base (options) {}

        public User GetByEmail(string email)
        {
            using(var context = Context)
            {
                return context.Users.SingleOrDefault(x => x.Email == email);
            }
        }
    }
}
