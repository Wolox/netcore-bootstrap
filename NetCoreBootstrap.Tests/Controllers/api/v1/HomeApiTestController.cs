using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using NetCoreBootstrap;
using Xunit;

namespace NetcoreBootstrap.Tests.Controllers.Api.V1
{
    public class HomeApiTestController
    {
        private readonly HttpClient _client;

        public HomeApiTestController()
        {
            var configuration = new ConfigurationBuilder()
                                .SetBasePath(Path.GetFullPath(@"../../../../NetCoreBootstrap/"))
                                .AddJsonFile("appsettings.Development.json", optional: false)
                                .Build();
            this._client = new TestServer(new WebHostBuilder()
                            .UseStartup<Startup>()
                            .UseConfiguration(configuration)).CreateClient();
        }

        public HttpClient Client
        {
            get { return this._client; }
        }

        [Fact]
        public async Task Get()
        {
            // Act
            Random rnd = new Random();
            int id = rnd.Next(1, 1000);
            var response = await Client.GetAsync($"/api/v1/homeapi/{id}");
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal($"{id}", responseString);
        }
    }
}
