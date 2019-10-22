using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bogus;
using Microsoft.AspNetCore.Http;
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
        public static IEnumerable<object[]> FakeUsers => new ModelList<UserSignUpVO>(FakerUserSignUpVO).GetModelList(5);

        [Theory]
        [MemberData(nameof(FakeUsers))]
        [Trait("Sign up", "Valid sign up")]
        public async Task ValidSignUp(UserSignUpVO userVO)
        {
            HttpClient.DefaultRequestHeaders.Clear();
            userVO.ConfirmPassword = userVO.Password;
            var response = await HttpClient.PostAsJsonAsync($"{AccountApiUrl}/SignUp", userVO);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [MemberData(nameof(FakeUsers))]
        [Trait("Sign up", "Invalid sign up - Passwords not matching")]
        public async Task PasswordsNotMatching(UserSignUpVO userVO)
        {
            HttpClient.DefaultRequestHeaders.Clear();
            userVO.ConfirmPassword = Guid.NewGuid().ToString();
            var response = await HttpClient.PostAsJsonAsync($"{AccountApiUrl}/SignUp", userVO);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
