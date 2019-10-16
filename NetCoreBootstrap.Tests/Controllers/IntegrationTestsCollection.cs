using Xunit;

namespace NetCoreBootstrap.Tests.Controllers
{
    [CollectionDefinition("Integration Tests Collection")]
    public class IntegrationTestsCollection : ICollectionFixture<IntegrationTestsFixture>
    {
        // Used to apply [CollectionDefinition] and ICollectionFixture<> interfaces.
    }
}
