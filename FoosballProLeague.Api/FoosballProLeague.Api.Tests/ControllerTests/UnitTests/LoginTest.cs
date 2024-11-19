using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FoosballProLeague.Api.BusinessLogic.Interfaces;

namespace FoosballProLeague.Api.Tests.ControllerTests.UnitTests;

public class LoginTest
{

    /* Unit test utilising Moq library to test if it correctly returns ok result with valid login credentials */
    [Fact]
    public void Login_ReturnsOkResult_WithValidCredentials()
    {
        // Arrange
        string validEmail = "john.doe@example.com";
        string validPassword = "password123";
        UserLoginModel mockUser = new UserLoginModel()
        {
            Email = validEmail,
            Password = validPassword,
        };

        Mock<IUserLogic> _mockUserLogic = new Mock<IUserLogic>(); ;
        _mockUserLogic.Setup(logic => logic.LoginUser(validEmail, validPassword)).Returns(true);

        Mock<ITokenLogic> _mockTokenLogic = new Mock<ITokenLogic>();

        UserController SUT = new UserController(_mockUserLogic.Object, _mockTokenLogic.Object);

        // Act
        IActionResult result = SUT.LoginUser(mockUser);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    /* Unit test utilising Moq library to test if it correctly returns bad result with invalid password but valid email */
    [Fact]
    public void Login_ReturnsBadRequest_WithInvalidPassword()
    {
        // Arrange
        string validEmail = "john.doe@example.com";
        string invalidPassword = "wrongpassword";

        UserLoginModel mockUser = new UserLoginModel()
        {
            Email = validEmail,
            Password = invalidPassword
        };

        Mock<IUserLogic> _mockUserLogic = new Mock<IUserLogic>(); ;
        _mockUserLogic.Setup(logic => logic.LoginUser(validEmail, invalidPassword)).Returns(false);

        Mock<ITokenLogic> _mockTokenLogic = new Mock<ITokenLogic>();

        UserController SUT = new UserController(_mockUserLogic.Object, _mockTokenLogic.Object);

        // Act
        IActionResult result = SUT.LoginUser(mockUser);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    /* Unit test utilising Moq library to test if it correctly returns bad result with invalid email but valid password */
    [Fact]
    public void Login_ReturnsBadRequest_WithInvalidEmail()
    {
        // Arrange
        string invalidEmail = "invalid@example.com";
        string validPassword = "password123";

        UserLoginModel mockUser = new UserLoginModel()
        {
            Email = invalidEmail,
            Password = validPassword
        };

        Mock<IUserLogic> _mockUserLogic = new Mock<IUserLogic>(); ;
        _mockUserLogic.Setup(logic => logic.LoginUser(invalidEmail, validPassword)).Returns(false);

        Mock<ITokenLogic> _mockTokenLogic = new Mock<ITokenLogic>();

        UserController SUT = new UserController(_mockUserLogic.Object, _mockTokenLogic.Object);

        // Act
        IActionResult result = SUT.LoginUser(mockUser);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
