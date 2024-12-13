using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.BusinessLogic.Interfaces;
using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.DatabaseAccess.Interfaces;
using FoosballProLeague.Api.Hubs;
using FoosballProLeague.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Moq;
using bc = BCrypt.Net.BCrypt;


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

            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();

            IConfiguration configuration = new ConfigurationManager();
            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            IUserLogic userLogic = new UserLogic(userDatabaseAccessor, mockHubContext.Object);
            ITokenLogic tokenLogic = new TokenLogic(configuration);

            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, mockHubContext.Object, userLogic, teamDatabaseAccessor);

            UserController SUT = new UserController(userLogic, tokenLogic, matchLogic);

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

            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();

            IConfiguration configuration = new ConfigurationManager();
            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            IUserLogic userLogic = new UserLogic(userDatabaseAccessor, mockHubContext.Object);
            ITokenLogic tokenLogic = new TokenLogic(configuration);

            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, mockHubContext.Object, userLogic, teamDatabaseAccessor);

            UserController SUT = new UserController(userLogic, tokenLogic, matchLogic);

            // Act: Call the LoginUser method
            IActionResult result = SUT.LoginUser(loginModel);

            // Assert: Verify that we get a BadRequest response
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}