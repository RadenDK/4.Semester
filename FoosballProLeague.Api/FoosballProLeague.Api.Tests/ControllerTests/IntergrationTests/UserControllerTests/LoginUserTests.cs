using Xunit;
using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.Models;
using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.DatabaseAccess;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;


namespace FoosballProLeague.Api.Tests.ControllerTests.IntegrationTests.UserControllerTests
{
    [Collection("Non-Parallel Database Collection")]

    public class LoginUserTests : DatabaseTestBase
    {
        private readonly UserController _userController;
        private readonly IUserLogic _userLogic;

        // initialize UserLogic and UserController
        public LoginUserTests()
        {
            _userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()));
            _userController = new UserController(_userLogic);
        }

        [Fact]
        public void LoginUser_SuccessfullLogin_ReturnsOk()
        {
            // Arrange: First clear the database 
            // then insert a user into the database
            _dbHelper.ClearDatabase();
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword("Password123");
            string insertQuery = @" INSERT INTO users (first_name, last_name, email, password, elo_1v1, elo_2v2)
                                VALUES ('John', 'Doe', 'john.doe@johndoe.com', '" + hashedPassword + "', 1500, 1600)";
            _dbHelper.InsertData(insertQuery);

            // Create a UserLoginModel object
            UserLoginModel loginModel = new UserLoginModel
            {
                Email = "john.doe@johndoe.com",
                Password = "Password123"
            };

            // Act: Call the LoginUser method
            IActionResult result = _userController.LoginUser(loginModel) as OkResult;

            // Assert: Verify the results
            Assert.IsType<OkResult>(result);
        }


        [Fact]
        public void LoginUser_WrongPassword_ReturnsBadRequest()
        {
            // Arrange: First clear the database 
            // then insert a user into the database
            _dbHelper.ClearDatabase();
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword("Password123");
            string insertQuery = @" INSERT INTO users (first_name, last_name, email, password, elo_1v1, elo_2v2)
                                VALUES ('John', 'Doe', 'john.doe@johndoe.com', '" + hashedPassword + "', 1500, 1600)";
            _dbHelper.InsertData(insertQuery);

            // Creating a UserLoginModel object with wrong password
            UserLoginModel loginModel = new UserLoginModel
            {
                Email = "john.doe@johndoe.com",
                Password = "WrongPassword"
            };

            // Act: Call the LoginUser method
            IActionResult result = _userController.LoginUser(loginModel);

            // Assert: Verify that we get a BadRequest response
            Assert.IsType<BadRequestResult>(result);

        }

    }
}