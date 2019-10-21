using Bogus;
using NetCoreBootstrap.Core.Models.VOs;

namespace NetCoreBootstrap.Tests.Faker
{
    public static class FakerDefinitions
    {
        public static Faker<UserSignUpVO> UserSignUpFaker { get; }
        public static Faker<UserSignInVO> UserSignInFaker { get; }

        static FakerDefinitions()
        {
            UserSignInFaker = new Faker<UserSignInVO>()
                .RuleFor(x => x.UserName, f => f.Internet.UserName())
                .RuleFor(x => x.Email, f => f.Internet.Email())
                .RuleFor(x => x.Password,
                    f => f.Random.String2(f.Random.Int(8, 16), "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+") + "aA_1");

            UserSignUpFaker = new Faker<UserSignUpVO>()
                .RuleFor(x => x.UserName, f => f.Internet.UserName())
                .RuleFor(x => x.Email, f => f.Internet.Email())
                .RuleFor(x => x.Password,
                    f => f.Random.String2(f.Random.Int(8, 16), "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+") + "aA_1");
        }
    }
}
