using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bc = BCrypt.Net.BCrypt;


namespace FoosballProLeague.Api.Tests.ControllerTests.IntergrationTests.UserControllerTests
{
    [Collection("Non-Parallel Database Collection")]
    public class ValidateUserJWT : DatabaseTestBase
    {
        [Fact]
        public void ValidateUserJWT_ShouldReturnTrue_ForAJwtAfterAValidLogin()
        {
            // Arrange
            string mockUserEmail = "johndoe@mail.com";
            string mockUserNonHashedPassword = "Password123";
            string mockUserHashedPassword = bc.HashPassword(mockUserNonHashedPassword);
            string insertUserQuery = $"INSERT INTO users (id, first_name, last_name, email, password, elo_1v1, elo_2v2) VALUES (1, 'John', 'Doe', '{mockUserEmail}', '{mockUserHashedPassword}', 500, 500)";

            _dbHelper.InsertData(insertUserQuery);

            UserLoginModel loginModel = new UserLoginModel
            {
                Email = mockUserEmail,
                Password = mockUserNonHashedPassword
            };

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            IUserLogic userLogic = new UserLogic(userDatabaseAccessor);
            ITokenLogic tokenLogic = new TokenLogic(_dbHelper.GetConfiguration());
            UserController SUT = new UserController(userLogic, tokenLogic);

            // Act: Login user and get JWT
            OkObjectResult loginResult = SUT.LoginUser(loginModel) as OkObjectResult;
            string jwt = loginResult?.Value as string;

            // Set up mock HTTP context for setting headers
            SUT.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            SUT.HttpContext.Request.Headers["Authorization"] = jwt;

            IActionResult result = SUT.ValidateUserJWT();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void ValidateUserJWT_ShouldReturnFalse_ForAJwtThatTheApiDidNotMake()
        {
            // Arrange

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            IUserLogic userLogic = new UserLogic(userDatabaseAccessor);
            ITokenLogic tokenLogic = new TokenLogic(_dbHelper.GetConfiguration());
            UserController SUT = new UserController(userLogic, tokenLogic);

            // Act: Login user and get JWT

            // Set up mock HTTP context for setting headers
            SUT.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            SUT.HttpContext.Request.Headers["Authorization"] = "MockJWTThatTheApiDidNotCreate";

            IActionResult result = SUT.ValidateUserJWT();

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public void ValidateUserJWT_ShouldReturnFalse_OnAnReqeustWithNoJWTInTheHeader()
        {
            // Arrange

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            IUserLogic userLogic = new UserLogic(userDatabaseAccessor);
            ITokenLogic tokenLogic = new TokenLogic(_dbHelper.GetConfiguration());
            UserController SUT = new UserController(userLogic, tokenLogic);

            // Act: Login user and get JWT

            // Set up mock HTTP context for setting headers
            SUT.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            SUT.HttpContext.Request.Headers["Authorization"] = "";

            IActionResult result = SUT.ValidateUserJWT();

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }


        [Fact]
        public void ValidateUserJWT_ShouldReturnFalse_OnAnReqeustWithNoAuthorizationHeader()
        {
            // Arrange

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            IUserLogic userLogic = new UserLogic(userDatabaseAccessor);
            ITokenLogic tokenLogic = new TokenLogic(_dbHelper.GetConfiguration());
            UserController SUT = new UserController(userLogic, tokenLogic);

            // Act: Login user and get JWT

            IActionResult result = SUT.ValidateUserJWT();

            // Assert

            // Two asserts just to check that it is not a ok response that it returns. Only because i dont really know if the test simulate a real request correct
            Assert.IsNotType<OkObjectResult>(result);
            Assert.IsNotType<OkResult>(result);

        }
    }
}
