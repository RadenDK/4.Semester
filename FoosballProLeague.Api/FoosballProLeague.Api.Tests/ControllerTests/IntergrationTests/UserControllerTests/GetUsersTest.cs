using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.Models;
using FoosballProLeague.Api.BusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using System.Collections.Generic;
using FoosballProLeague.Api.DatabaseAccess;

namespace FoosballProLeague.Api.Tests.ControllerTests.IntegrationTests
{
    [Collection("Non-Parallel Database Collection")]
    public class GetUsersTest : DatabaseTestBase
    {

        [Fact]
        public void GetUsers_ReturnsAllUsers()
        {
            // Arrange: Insert test data into the database
            string insertQuery = @"
                INSERT INTO users (first_name, last_name, email, password, elo_1v1, elo_2v2)
                VALUES ('John', 'Doe', 'john.doe@example.com', 'hashedpassword', 1500, 1600),
                       ('Jane', 'Smith', 'jane.smith@example.com', 'hashedpassword', 1400, 1500)";
            _dbHelper.InsertData(insertQuery);

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITokenLogic tokenLogic = new TokenLogic(_dbHelper.GetConfiguration());
            IUserLogic userLogic = new UserLogic(userDatabaseAccessor);
            UserController SUT = new UserController(userLogic, tokenLogic);

            // Act: Call the GetUsers method
            IActionResult result = SUT.GetUsers() as OkObjectResult;
            IEnumerable<UserModel> users = (result as OkObjectResult)?.Value as IEnumerable<UserModel>;

            // Assert: Verify the results
            Assert.NotNull(users);
            Assert.Equal(2, users.Count());

            UserModel user1 = users.First();
            UserModel user2 = users.Last();

            Assert.NotNull(user1);
            Assert.Equal("John", user1.FirstName);
            Assert.Equal("Doe", user1.LastName);
            Assert.Equal(1500, user1.Elo1v1);
            Assert.Equal(1600, user1.Elo2v2);

            Assert.NotNull(user2);
            Assert.Equal("Jane", user2.FirstName);
            Assert.Equal("Smith", user2.LastName);
            Assert.Equal(1400, user2.Elo1v1);
            Assert.Equal(1500, user2.Elo2v2);
        }

        [Fact]
        public void GetUsers_ReturnsEmptyList_WhenNoUsersExist()
        {
            // Arrange: Ensure the database is empty
            _dbHelper.ClearDatabase();

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITokenLogic tokenLogic = new TokenLogic(_dbHelper.GetConfiguration());
            IUserLogic userLogic = new UserLogic(userDatabaseAccessor);
            UserController SUT = new UserController(userLogic, tokenLogic);

            // Act: Call the GetUsers method
            IActionResult result = SUT.GetUsers() as OkObjectResult;
            IEnumerable<UserModel> users = (result as OkObjectResult)?.Value as IEnumerable<UserModel>;

            // Assert: Verify the results
            Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(users);
            Assert.Empty(users);
        }
    }
}
