using Xunit;
using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.Models;
using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.DatabaseAccess;
using bc = BCrypt.Net.BCrypt;
using Microsoft.AspNetCore.Mvc;

using FoosballProLeague.Api.DatabaseAccess.Interfaces;
using FoosballProLeague.Api.BusinessLogic.Interfaces;


namespace FoosballProLeague.Api.Tests.ControllerTests.IntegrationTests.UserControllerTests
{
    [Collection("Non-Parallel Database Collection")]

    public class LoginUserTests : DatabaseTestBase
    {

        [Fact]
        public void LoginUser_SuccessfullLogin_ReturnsOk()
        {
            // Arrange
            // then insert a user into the database
            string nonHashedPassword = "Password123";
            string hashedPassword = bc.HashPassword(nonHashedPassword);
            string insertQuery = @" INSERT INTO users (first_name, last_name, email, password, elo_1v1, elo_2v2)
                                VALUES ('John', 'Doe', 'john.doe@johndoe.com', '" + hashedPassword + "', 1500, 1600)";
            _dbHelper.InsertData(insertQuery);

            // Create a UserLoginModel object
            UserLoginModel loginModel = new UserLoginModel
            {
                Email = "john.doe@johndoe.com",
                Password = nonHashedPassword
            };

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITokenLogic tokenLogic = new TokenLogic(_dbHelper.GetConfiguration());
            IUserLogic userLogic = new UserLogic(userDatabaseAccessor);
            UserController SUT = new UserController(userLogic, tokenLogic);

            // Act: Call the LoginUser method
            IActionResult result = SUT.LoginUser(loginModel);

            // Assert: Verify the results
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void LoginUser_WrongPassword_ReturnsBadRequest()
        {
            // Arrange: First clear the database 
            // then insert a user into the database
            string hashedPassword = bc.HashPassword("Password123");
            string insertQuery = @" INSERT INTO users (first_name, last_name, email, password, elo_1v1, elo_2v2)
                                VALUES ('John', 'Doe', 'john.doe@johndoe.com', '" + hashedPassword + "', 1500, 1600)";
            _dbHelper.InsertData(insertQuery);

            // Creating a UserLoginModel object with wrong password
            UserLoginModel loginModel = new UserLoginModel
            {
                Email = "john.doe@johndoe.com",
                Password = "WrongPassword"
            };

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITokenLogic tokenLogic = new TokenLogic(_dbHelper.GetConfiguration());
            IUserLogic userLogic = new UserLogic(userDatabaseAccessor);
            UserController SUT = new UserController(userLogic, tokenLogic);

            // Act: Call the LoginUser method
            IActionResult result = SUT.LoginUser(loginModel);

            // Assert: Verify that we get a BadRequest response
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}