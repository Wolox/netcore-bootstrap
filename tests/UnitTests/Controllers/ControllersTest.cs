using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NetCoreBootstrap.Controllers;
using NetCoreBootstrap.Controllers.api.v1;
using NetCoreBootstrap.Models;
using NetCoreBootstrap.Models.Database;


using Xunit;

namespace TestingControllersSample.Tests.UnitTests
{
    public class HomeControllerTests
    {
        [Fact]
        public void ApiController_Get_ReturnsAString_WithValue()
        {
            // Arrange
            var controller = new HomeApiController();//mockRepo.Object

            // Act
            int id = 1;
            var result = controller.Get(id);
            

            // Assert
            var viewResult = Assert.IsType<String>(result);
            Assert.Equal("value", result);
        }
    }
    
    public class TestingEndpointsController
    {
        [Fact]
        public void TestingEndpoints_Returns_Value()
        {
            Random Random = new Random();
            var Controller = new TestingEndpoints();
            int Number = Random.Next();
            var GetNumberResult = Controller.Get(Number);
            Assert.IsType<int>(GetNumberResult);
            Assert.Equal(Number,GetNumberResult);

            int day = Random.Next(01,28);
            int month = Random.Next(1,12);
            int year = Random.Next(2017,2020);

            var DateConvertResult = Controller.DateConvert(year,month,day);
            var Date = new DateTime(year,month,day);
            Assert.IsType<JsonResult>(DateConvertResult);
            
        }
    }
}