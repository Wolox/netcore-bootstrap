using MyMVCProject.Models.Database;
using MyMVCProject.Models.View;

namespace MyMVCProject.Mappers
{
    public static class UserViewModelMapper
    {
        public static UserViewModel MapFrom(User user)
        {
            return new UserViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };
        }
    }
}
