using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using NetCoreBootstrap.Core.Models.Database;
using NetCoreBootstrap.Data.Repositories.Database;
using NetCoreBootstrap.Services;
using NetCoreBootstrap.Services.Helpers;
using NetCoreBootstrap.Services.Intefaces;
using Xunit;

namespace NetCoreBootstrap.Tests.Controllers
{
    public class IntegrationTestsFixture : ICollectionFixture<WebHostBuilder>
    {
        private readonly HttpClient _client;
        private readonly HttpClient _secondaryClient;
        private readonly UnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly TestServer _server;
        private readonly AccountHelper _accountHelper;

        public IntegrationTestsFixture()
        {
            _configuration = new ConfigurationBuilder()
                    .SetBasePath(Path.GetFullPath(@"../../NetCoreBootstrap.Tests"))
                    .AddJsonFile("../../NetCoreBootstrap.Tests/appsettings.Testing.json", optional: false)
                    .Build();
            var builder = new WebHostBuilder()
                    .UseEnvironment("Testing")
                    .UseStartup("../../NetCoreBootstrap.Api/Startup.cs")
                    .UseConfiguration(_configuration);
            this._server = new TestServer(builder);
            this._userManager = TestServer.Host.Services.GetService(typeof(UserManager<User>)) as UserManager<User>;
            this._roleManager = TestServer.Host.Services.GetService(typeof(RoleManager<IdentityRole>)) as RoleManager<IdentityRole>;
            this._unitOfWork = new UnitOfWork(TestServer.Host.Services.GetService(typeof(DatabaseContext)) as DatabaseContext, _userManager, _roleManager);
            this._client = TestServer.CreateClient();
            this._secondaryClient = TestServer.CreateClient();
            this._accountHelper = new AccountHelper(_configuration,
                                                    TestServer.Host.Services.GetService(typeof(IMailer)) as Mailer,
                                                    TestServer.Host.Services.GetService(typeof(IHtmlLocalizer<AccountHelper>)) as IHtmlLocalizer<AccountHelper>);
        }

        public HttpClient Client => _client;
        public HttpClient SecondaryClient => _secondaryClient;
        public UnitOfWork UnitOfWork => _unitOfWork;
        public UserManager<User> UserManager => _userManager;
        public IConfiguration Configuration => _configuration;
        public TestServer TestServer => _server;
        public AccountHelper AccountHelper => _accountHelper;

        protected Dictionary<string, string> TokenVariables =>
            new Dictionary<string, string>
            {
                { "key", Configuration["Jwt:Key"] },
                { "expire", Configuration["Jwt:ExpireDays"] },
                { "issuer", Configuration["Jwt:Issuer"] },
            };

    }
}
