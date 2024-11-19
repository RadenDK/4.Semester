using FoosballProLeague.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using FoosballProLeague.Api.DatabaseAccess;
using Moq;
using FoosballProLeague.Api.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.SignalR;
using FoosballProLeague.Api.Hubs;

namespace FoosballProLeague.Api.Tests.ControllerTests.UnitTests
{
    [Collection("Non-Parallel Database Collection")]
    public class GetUsersTest : DatabaseTestBase
    {
        private readonly UserController _userController;
        private readonly IUserLogic _userLogic;

        public GetUsersTest()
        {
            // Mock IHubContext and IHubClients
            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();
        }

        [Fact]
        public void GetLeaderboards_ReturnsBadRequest_WhenExceptionThrown()
        {
            // Arrange: Create a mock of IUserLogic that throws an exception
            Mock<IUserLogic> mockUserLogic = new Mock<IUserLogic>();
            Mock<ITokenLogic> mockTokenLogic = new Mock<ITokenLogic>();
            mockUserLogic.Setup(logic => logic.GetLeaderboards()).Throws(new Exception("Test exception"));

            UserController SUT = new UserController(mockUserLogic.Object, mockTokenLogic.Object);

            // Act: Call the GetUsers method
            IActionResult result = SUT.GetLeaderboards();

            // Assert: Verify the results
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}