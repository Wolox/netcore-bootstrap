using System.Linq;
using MyMVCProject.Models.Database;

namespace MyMVCProject.Respositories
{
    public class UserRepository : MasterRepository<User>
    {
        public User GetByEmail(string email)
        {
            using(var context = new DataBaseContext())
            {
                return context.Users.SingleOrDefault(x => x.Email == email);
            }
        }
    }
}