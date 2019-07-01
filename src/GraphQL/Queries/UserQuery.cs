using GraphQL.Types;
using NetCoreBootstrap.Repositories.Interfaces;
using NetCoreBootstrap.GraphQL.GraphQLTypes;

namespace NetCoreBootstrap.GraphQL.Queries
{
    public class UserQuery : ObjectGraphType
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserQuery(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
            Field<ListGraphType<UserType>>("users", resolve: context => _unitOfWork.UserRepository.GetAllUsers());
            Field<UserType>("user",
                            arguments: new QueryArguments(new QueryArgument<IntGraphType> { Name = "id" }),
                            resolve:
                                context => _unitOfWork.UserRepository
                                                    .GetUserById(context.GetArgument<string>("id"))
                                                    .Result);
        }
    }
}
