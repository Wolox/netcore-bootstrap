using GraphQL;
using GraphQL.Types;
using NetCoreBootstrap.GraphQL.Queries;

namespace NetCoreBootstrap.GraphQL.Schemas
{
    public class UserSchema : Schema
    {
        public UserSchema(IDependencyResolver resolver) : base(resolver)
            => this.Query = resolver.Resolve<UserQuery>();
    }
}
