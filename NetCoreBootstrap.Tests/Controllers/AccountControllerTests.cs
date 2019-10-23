using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bogus;
using Microsoft.AspNetCore.Http;
using NetCoreBootstrap.Core.Models.Database;
using NetCoreBootstrap.Core.Models.VOs;
using NetCoreBootstrap.Tests.Faker;
using NetCoreBootstrap.Tests.Faker.Strategies;
using Xunit;

namespace NetCoreBootstrap.Tests.Controllers
{
    [Collection("Integration Tests Collection")]
    public class AccountControllerTests : IntegrationTests
    {
        private readonly HttpClient _client;
        private const string AccountApiUrl = "/api/Account";

        public AccountControllerTests(IntegrationTestsFixture fixture) : base(fixture)
        {
            _client = fixture.Client;
        }

        public HttpClient HttpClient => _client;
        public static Faker<UserSignUpVO> FakerUserSignUpVO => FakerDefinitions.UserSignUpFaker;
        public static Faker<UserSignInVO> FakerUserSignInVO => FakerDefinitions.UserSignInFaker;
        public static IEnumerable<object[]> FakeUsersToSignUp => new ModelList<UserSignUpVO>(FakerUserSignUpVO).GetModelList(1);
        public static IEnumerable<object[]> FakeUsersToSignIn => new ModelList<UserSignInVO>(FakerUserSignInVO).GetModelList(1);


        [Theory]
        [MemberData(nameof(FakeUsersToSignUp))]
        [Trait("Sign up", "Valid sign up")]
        public async Task ValidSignUp(UserSignUpVO userVO)
        {
            HttpClient.DefaultRequestHeaders.Clear();
            userVO.ConfirmPassword = userVO.Password;
            var response = await HttpClient.PostAsJsonAsync($"{AccountApiUrl}/SignUp", userVO);
            Assert.True(response.IsSuccessStatusCode);
        }

        [Theory]
        [MemberData(nameof(FakeUsersToSignUp))]
        [Trait("Sign up", "Invalid sign up - Passwords not matching")]
        public async Task PasswordsNotMatching(UserSignUpVO userVO)
        {
            HttpClient.DefaultRequestHeaders.Clear();
            userVO.ConfirmPassword = Guid.NewGuid().ToString();
            var response = await HttpClient.PostAsJsonAsync($"{AccountApiUrl}/SignUp", userVO);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(FakeUsersToSignIn))]
        [Trait("Sign in", "Valid sign in")]
        public async Task ValidSignIn(UserSignInVO userVO)
        {
            HttpClient.DefaultRequestHeaders.Clear();
            var user = new User
            {
                UserName = userVO.Email.ToLower(),
                Email = userVO.Email.ToLower(),
            };
            await UserManager.CreateAsync(user, userVO.Password);
            user.EmailConfirmed = true;
            await UserManager.UpdateAsync(user);
            var response = await HttpClient.PostAsJsonAsync($"{AccountApiUrl}/SignIn", userVO);
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(response.Content);
        }
    }
}
