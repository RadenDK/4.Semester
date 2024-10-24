using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.Models;
using FoosballProLeague.Api.BusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Moq;
using BCrypt.Net;
using Microsoft.AspNetCore.Identity.Data;

namespace FoosballProLeague.Api.Tests.ControllerTests.UnitTests;

public class LoginTest
{
    private readonly UserController _userController;
    private readonly Mock<IUserLogic> _mockUserLogic;

    public LoginTest()
    {
        _mockUserLogic = new Mock<IUserLogic>();
        _userController = new UserController(_mockUserLogic.Object);
    }

    /* Unit test utilising Moq library to test if it correctly returns ok result with valid login credentials */
    [Fact]
    public void Login_ReturnsOkResult_WithValidCredentials()
    {
        // Arrange
        string validEmail = "john.doe@example.com";
        string validPassword = "password123";
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(validPassword);
        UserLoginModel user = new UserLoginModel()
        {
            Email = validEmail,
            Password = hashedPassword,
        };

        _mockUserLogic.Setup(logic => logic.LoginUser(validEmail, validPassword)).Returns(true);

        // Act
        OkResult result = _userController.LoginUser(new UserLoginModel { Email = validEmail, Password = validPassword }) as OkResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
    }
    
    /* Unit test utilising Moq library to test if it correctly returns bad result with invalid password but valid email */
    [Fact]
    public void Login_ReturnsBadRequest_WithInvalidPassword()
    {
        // Arrange
        string validEmail = "john.doe@example.com";
        string validPassword = "password123";
        string invalidPassword = "wrongpassword";
        
        UserLoginModel user = new UserLoginModel()
        {
            Email = validEmail,
            Password = invalidPassword
        };

        _mockUserLogic.Setup(logic => logic.LoginUser(validEmail, invalidPassword)).Returns(false);

        // Act
        BadRequestResult result = _userController.LoginUser(new UserLoginModel { Email = validEmail, Password = invalidPassword }) as BadRequestResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }

    /* Unit test utilising Moq library to test if it correctly returns bad result with invalid email but valid password */
    [Fact]
    public void Login_ReturnsBadRequest_WithInvalidEmail()
    {
        // Arrange
        string invalidEmail = "invalid@example.com";
        string validPassword = "password123";

        UserLoginModel user = new UserLoginModel()
        {
            Email = invalidEmail,
            Password = validPassword
        };

        _mockUserLogic.Setup(logic => logic.LoginUser(invalidEmail, validPassword)).Returns(false);

        // Act
        BadRequestResult result = _userController.LoginUser(new UserLoginModel { Email = invalidEmail, Password = validPassword }) as BadRequestResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
}
