using Bogus;
using NetCoreBootstrap.Core.Models.VOs;

namespace NetCoreBootstrap.Tests.Faker
{
    public class FakerRules
    {
        public Faker<UserSignUpVO> UserSignUpFaker { get; }
        public Faker<UserSignInVO> UserSignInFaker { get; }

        public FakerRules()
        {
            this.UserSignInFaker = new Faker<UserSignInVO>()
                .RuleFor(x => x.UserName, f => f.Internet.UserName())
                .RuleFor(x => x.Email, f => f.Internet.Email())
                .RuleFor(x => x.Password, f => f.Random
                    .String2(f.Random.Int(8, 16), "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+") + "aA_1");

            // TODO: UserSignUpFaker + List strategies

        }
    }
}