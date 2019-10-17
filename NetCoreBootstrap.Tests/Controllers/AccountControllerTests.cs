using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bogus;
using Microsoft.AspNetCore.Http;
using NetCoreBootstrap.Core.Models.VOs;
using NetCoreBootstrap.Tests.Faker;
using Xunit;

namespace NetCoreBootstrap.Tests.Controllers
{
    [Collection("Integration tests collection")]
    public class AccountControllerTests : IntegrationTests
    {
        private readonly HttpClient _secondaryClient;
        private readonly FakerDefinitions _fakerDefinitions;

        public AccountControllerTests(IntegrationTestsFixture fixture) : base(fixture)
        {
            _secondaryClient = fixture.SecondaryClient;
            _fakerDefinitions = new FakerDefinitions();
        }

        public HttpClient SecondaryClient => _secondaryClient;
        public Faker<UserSignUpVO> FakerUserSignUpVO => _fakerDefinitions.UserSignUpFaker;

        [Theory]
        [MemberData(nameof(FakerUserSignUpVO))]
        public async Task ValidSignUp(UserSignUpVO userVO)
        {
            SecondaryClient.DefaultRequestHeaders.Clear();
            userVO.ConfirmPassword = userVO.Password;
            var response = await SecondaryClient.PostAsJsonAsync($"/api/Account/SignUp", userVO);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}