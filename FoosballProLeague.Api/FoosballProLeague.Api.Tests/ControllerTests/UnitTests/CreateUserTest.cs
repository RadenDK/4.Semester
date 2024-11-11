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
    private readonly UserController _userController;
    private readonly Mock<IUserLogic> _mockUserLogic;

    public CreateUserTest()
    {
        _mockUserLogic = new Mock<IUserLogic>();
        _userController = new UserController(_mockUserLogic.Object);
    }
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

        _mockUserLogic.Setup(logic => logic.CreateUser(validUser)).Returns(true);

        // Act
        IActionResult result = await _userController.CreateUser(validUser);

        // Assert
        OkResult okResult = result as OkResult;
        Assert.NotNull(okResult);
        Assert.Equal(200, okResult.StatusCode);
    }
}