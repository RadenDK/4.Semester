using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.Hubs;
using FoosballProLeague.Api.Models;
using Microsoft.AspNetCore.Mvc;
using FoosballProLeague.Api.BusinessLogic.Interfaces;
using FoosballProLeague.Api.DatabaseAccess.Interfaces;
using Moq;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;

namespace FoosballProLeague.Api.Tests.ControllerTests.IntegrationTests
{
    [Collection("Non-Parallel Database Collection")]

    public class CreateUserTest : DatabaseTestBase
    {

        [Fact]
        public async void CreateUser_ShouldReturnOk()
        {
            // Arrange
            string insertCompanyQuery = "INSERT INTO companies (id, name) VALUES (1, 'Test Company')";
            _dbHelper.InsertData(insertCompanyQuery);

            string insertDepartmentQuery = "INSERT INTO departments (name, company_id) VALUES ('Test Department', 1)";
            _dbHelper.InsertData(insertDepartmentQuery);

            UserRegistrationModel newUser = new UserRegistrationModel
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Password = "Password123!",
                DepartmentId = 1,
                CompanyId = 1
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
            // Act
            IActionResult result = SUT.CreateUser(newUser) as OkResult;

            // Assert
            IEnumerable<UserModel> users = _dbHelper.ReadData<UserModel>("SELECT * FROM users");
            UserModel user = users.First();


            Assert.IsAssignableFrom<OkResult>(result);
            Assert.Equal(1, users.Count());
            Assert.Equal("John", user.FirstName);
            Assert.Equal("Doe", user.LastName);
            Assert.Equal("john.doe@example.com", user.Email);
            Assert.Equal(500, user.Elo1v1); // The database should assign default values for Elo1v1 and Elo2v2
            Assert.Equal(500, user.Elo2v2); // The database should assign default values for Elo1v1 and Elo2v2
        }

        [Fact]
        public async void CreateUser_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            string insertCompanyQuery = "INSERT INTO companies (id, name) VALUES (1, 'Test Company')";
            _dbHelper.InsertData(insertCompanyQuery);

            string insertDepartmentQuery = "INSERT INTO departments (name, company_id) VALUES ('Test Department', 1)";
            _dbHelper.InsertData(insertDepartmentQuery);

            UserRegistrationModel newUser = new UserRegistrationModel
            {
                FirstName = "John",
                LastName = "Doe",
                Password = "Password123!",
                DepartmentId = 1,
                CompanyId = 1
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

            SUT.ModelState.AddModelError("Email", "Email is required");

            // Act
            IActionResult result = SUT.CreateUser(newUser) as BadRequestObjectResult;

            // Assert
            IEnumerable<UserModel> users = _dbHelper.ReadData<UserModel>("SELECT * FROM users");

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Empty(users);
        }

        [Fact]
        public async void CreateUser_ShouldReturnBadRequest_WhenEmailAlreadyExists()
        {
            // Arrange
            string insertCompanyQuery = "INSERT INTO companies (id, name) VALUES (1, 'Test Company')";
            _dbHelper.InsertData(insertCompanyQuery);

            string insertDepartmentQuery = "INSERT INTO departments (name, company_id) VALUES ('Test Department', 1)";
            _dbHelper.InsertData(insertDepartmentQuery);

            string insertUserQuery = @"INSERT INTO users (first_name, last_name, email, password, department_id, company_id, elo_1v1, elo_2v2)
                                    VALUES ('Existing', 'User', 'existing.user@example.com', 'Password123!', 1, 1, 500, 500)";
            _dbHelper.InsertData(insertUserQuery);

            UserRegistrationModel newUser = new UserRegistrationModel
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "existing.user@example.com",
                Password = "Password123!",
                DepartmentId = 1,
                CompanyId = 1
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

            // Act
            IActionResult result = SUT.CreateUser(newUser) as BadRequestObjectResult;

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}