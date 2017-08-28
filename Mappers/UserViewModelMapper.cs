using NetCoreBootstrap.Models.Database;
using NetCoreBootstrap.Models.View;

namespace NetCoreBootstrap.Mappers
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
