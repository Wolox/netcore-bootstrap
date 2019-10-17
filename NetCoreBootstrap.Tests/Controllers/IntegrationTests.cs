using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Identity;
using NetCoreBootstrap.Core.Models.Database;
using NetCoreBootstrap.Data.Repositories.Interfaces;
using System.Collections.Generic;

namespace NetCoreBootstrap.Tests.Controllers
{
    public class IntegrationTests
    {
        private readonly HttpClient _client;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly TestServer _server;

        public IntegrationTests(IntegrationTestsFixture fixture)
        {
            _client = fixture.Client;
            _unitOfWork = fixture.UnitOfWork;
            _userManager = fixture.UserManager;
            _configuration = fixture.Configuration;
            _server = fixture.TestServer;
        }

        public HttpClient Client => _client;
        public IUnitOfWork UnitOfWork => _unitOfWork;
        public UserManager<User> UserManager => _userManager;
        public IConfiguration Configuration => _configuration;
        public TestServer TestServer => _server;

        protected Dictionary<string, string> TokenVariables =>
            new Dictionary<string, string>
            {
                { "key", Configuration["Jwt:Key"] },
                { "expire", Configuration["Jwt:ExpireDays"] },
                { "issuer", Configuration["Jwt:Issuer"] },
            };
    }
}
