using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.Models;
using FoosballProLeague.Api.BusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using System.Collections.Generic;
using FoosballProLeague.Api.DatabaseAccess;
using Moq;

namespace FoosballProLeague.Api.Tests.ControllerTests.UnitTests
{
    [Collection("Non-Parallel Database Collection")]
    public class GetUsersTest : DatabaseTestBase
    {
       
        [Fact]
        public void GetUsers_ReturnsBadRequest_WhenExceptionThrown()
        {
            // Arrange: Create a mock of IUserLogic that throws an exception
            Mock<IUserLogic> mockUserLogic = new Mock<IUserLogic>();
            Mock<ITokenLogic> mockTokenLogic = new Mock<ITokenLogic>();
            mockUserLogic.Setup(logic => logic.GetUsers()).Throws(new Exception("Test exception"));

            UserController SUT = new UserController(mockUserLogic.Object, mockTokenLogic.Object);

            // Act: Call the GetUsers method
            IActionResult result = SUT.GetUsers();

            // Assert: Verify the results
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
