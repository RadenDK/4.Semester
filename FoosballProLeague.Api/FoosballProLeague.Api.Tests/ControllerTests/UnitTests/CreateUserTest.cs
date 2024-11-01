using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.Models;
using FoosballProLeague.Api.BusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Moq;
using BCrypt.Net;
using Microsoft.AspNetCore.Identity.Data;

namespace FoosballProLeague.Api.Tests.ControllerTests.UnitTests;

public class CreateUserTest
{
    [Fact]
    public void CreateUser_ReturnsOkResult_WithValidData()
    {
        // Arrange
        UserRegistrationModel validUser = new UserRegistrationModel
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "password123"
        };

        Mock<IUserLogic> _mockUserLogic = new Mock<IUserLogic>();
        _mockUserLogic.Setup(logic => logic.CreateUser(validUser)).Returns(true);

        Mock<ITokenLogic> _mockTokenLogic = new Mock<ITokenLogic>();

        UserController SUT = new UserController(_mockUserLogic.Object, _mockTokenLogic.Object);

        // Act
        IActionResult result = SUT.CreateUser(validUser) as OkResult;

        // Assert
        Assert.IsType<OkResult>(result);
    }
}