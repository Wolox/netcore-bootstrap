using System.Linq;
using MyMVCProject.Models.Database;

namespace MyMVCProject.Respositories
{
    public class UserRepository : MasterRepository<User>
    {
        public UserRepository(DataBaseContext context) : base (context) { }

        public User GetByEmail(string email)
        {
            using(var context = getContext())
            {
                return context.Users.SingleOrDefault(x => x.Email == email);
            }
        }
    }
}
