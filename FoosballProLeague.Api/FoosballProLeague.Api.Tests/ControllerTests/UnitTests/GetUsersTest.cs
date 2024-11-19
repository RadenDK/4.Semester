using FoosballProLeague.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using FoosballProLeague.Api.DatabaseAccess;
using Moq;
using FoosballProLeague.Api.BusinessLogic.Interfaces;

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
            mockUserLogic.Setup(logic => logic.GetAllUsers()).Throws(new Exception("Test exception"));

            UserController SUT = new UserController(mockUserLogic.Object, mockTokenLogic.Object);

            // Act: Call the GetUsers method
            IActionResult result = SUT.GetUsers();

            // Assert: Verify the results
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
