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
        private readonly UserController _userController;
        private readonly IUserLogic _userLogic;

        public GetUsersTest()
        {
            // Initialize UserLogic and UserController
            _userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()));
            _userController = new UserController(_userLogic);
        }

        [Fact]
        public void GetUsers_ReturnsBadRequest_WhenExceptionThrown()
        {
            // Arrange: Create a mock of IUserLogic that throws an exception
            var mockUserLogic = new Mock<IUserLogic>();
            mockUserLogic.Setup(logic => logic.GetUsers()).Throws(new Exception("Test exception"));

            var controller = new UserController(mockUserLogic.Object);

            // Act: Call the GetUsers method
            var result = controller.GetUsers() as BadRequestObjectResult;

            // Assert: Verify the results
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Test exception", result.Value);
        }
    }
}
