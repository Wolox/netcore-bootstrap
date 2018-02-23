using Microsoft.EntityFrameworkCore;
using Moq;
using NetCoreBootstrap.Controllers.Api.V1;
using NetCoreBootstrap.Models.Database;
using NetCoreBootstrap.Persistance;
using NetCoreBootstrap.Persistance.Interfaces;
using Xunit;


namespace NetcoreBootstrap.Tests.Controllers.api.v1
{
    public class ExampleApiTestController
    {
        [Fact]
        public void Get()
        {
        //Given
        var repoMock = new Mock<IExampleModelRepository>();
        var uowMock = new Mock<IUnitOfWork>();
        repoMock.Setup(repo => repo.Get(1)).Returns(new ExampleModel{Attribute1 = 1, Attribute2 = "lalal", Attribute3 = true});
        uowMock.Setup(uow => uow.ExampleModelRepository).Returns(repoMock.Object);
        var controller = new ExampleModelApiController(uowMock.Object);
        //When
        var result = controller.Get(1);
        //Then
        Assert.Equal(1,1);
        }
    }
}