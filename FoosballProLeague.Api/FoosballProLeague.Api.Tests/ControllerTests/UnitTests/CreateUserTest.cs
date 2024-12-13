using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FoosballProLeague.Api.BusinessLogic.Interfaces;

namespace FoosballProLeague.Api.Tests.ControllerTests.UnitTests;

public class CreateUserTest
{
    [Fact]
    public async void CreateUser_ReturnsOkResult_WithValidData()
    {
        // Arrange
        UserRegistrationModel validUser = new UserRegistrationModel
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "password123"
        };

        Mock<IUserLogic> mockUserLogic = new Mock<IUserLogic>();
        mockUserLogic.Setup(logic => logic.CreateUser(validUser)).Returns(true);

        Mock<ITokenLogic> mockTokenLogic = new Mock<ITokenLogic>();

        Mock<IMatchLogic> mockMatchLogic = new Mock<IMatchLogic>();

        UserController SUT = new UserController(mockUserLogic.Object, mockTokenLogic.Object, mockMatchLogic.Object);

        // Act
        IActionResult result = SUT.CreateUser(validUser) as OkResult;

        // Assert
        Assert.IsType<OkResult>(result);
    }
}