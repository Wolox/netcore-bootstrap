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

namespace NetcoreBootstrap.Tests
{
    public class PrimeWebDefaultRequestShould
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;
        public PrimeWebDefaultRequestShould()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.GetFullPath(@"../../../../NetcoreBootstrap/"))
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
            _server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>()
                .UseConfiguration(configuration));
            _client = _server.CreateClient();
        }

        

        [Fact]
        public async Task ReturnHelloWorld()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/Test");
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal("Hello World!",
                responseString);
        }
    }
}
