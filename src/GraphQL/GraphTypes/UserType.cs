using GraphQL.Types;
using NetCoreBootstrap.Models.Database;

namespace NetCoreBootstrap.GraphQL.GraphQLTypes
{
    public class UserType : ObjectGraphType<User>
    {
        public UserType()
        {
            Field(user => user.Id);
            Field(user => user.Email);
            Field(user => user.UserName);
        }
    }
}
