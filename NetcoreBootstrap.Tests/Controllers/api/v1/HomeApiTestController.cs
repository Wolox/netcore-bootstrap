using System;
using Xunit;
using NetcoreBootstrap;
using System.Net.Http;
using NetCoreBootstrap;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace NetcoreBootstrap.Tests.Controllers.api.v1
{
    public class HomeApiTestController
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public HomeApiTestController()
        {
            var configuration = new ConfigurationBuilder()
                                .SetBasePath(Path.GetFullPath(@"../../../../NetcoreBootstrap/"))
                                .AddJsonFile("appsettings.Development.json", optional: false)
                                .Build();
            this._server = new TestServer(new WebHostBuilder()
                            .UseStartup<Startup>()
                            .UseConfiguration(configuration));
            this._client = this._server.CreateClient();
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
            int id = rnd.Next(1,1000);
            var response = await Client.GetAsync($"/api/v1/homeapi/{id}");
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal($"{id}", responseString);
        }
    }
}
