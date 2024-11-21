
using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.BusinessLogic.Interfaces;
using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.Hubs;
using FoosballProLeague.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Moq;

namespace FoosballProLeague.Api.Tests.ControllerTests.IntergrationTests.UserControllerTests
{
    [Collection("Non-Parallel Database Collection")]
    public class GetMatchHistoryByUserIdTest : DatabaseTestBase
    {
        [Fact]
        public void GetMatchHistoryByUserId_ShouldReturnMatchHistory()
        {
            // Arrange
            int userId = 1;
            _dbHelper.InsertData("INSERT INTO users (id, elo_1v1, elo_2v2) VALUES (1, 1000, 1000), (2, 1000, 1000), (3, 1000, 1000), (4, 1000, 1000)");
            _dbHelper.InsertData("INSERT INTO teams (id, player1_id, player2_id) VALUES (1, 1, 2), (2, 3, 4)");
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");
            _dbHelper.InsertData("INSERT INTO foosball_matches (id, table_id, start_time, end_time, red_team_id, blue_team_id, team_red_score, team_blue_score) VALUES (1, 1, '2024-11-18 01:05:49.460277', '2024-11-18 08:55:53.233263', 1, 2, 9, 0)");
            _dbHelper.UpdateData("UPDATE foosball_tables SET active_match_id = 1 WHERE id = 1");

            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();

            IConfiguration configuration = new ConfigurationManager();
            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHubContext.Object);
            ITokenLogic tokenLogic = new TokenLogic(configuration);
            UserController SUT = new UserController(userLogic, tokenLogic);
            
            // Act
            IActionResult result = SUT.GetMatchHistoryByUserId(userId);
            
            // Assert
            Assert.IsType<OkObjectResult>(result);
            OkObjectResult okResult = result as OkObjectResult;
            List<MatchHistoryModel> matchHistory = okResult.Value as List<MatchHistoryModel>;
            Assert.NotNull(matchHistory);
            Assert.Single(matchHistory);
            Assert.Equal(1, matchHistory[0].Id);
            
        }
        
        [Fact]
        public void GetMatchHistoryByUserId_NoMatchHistory_ShouldReturnNotFound()
        {
            // Arrange
            int userId = 1;
            _dbHelper.InsertData("INSERT INTO users (id, elo_1v1, elo_2v2) VALUES (1, 1000, 1000)");

            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();

            IConfiguration configuration = new ConfigurationManager();
            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHubContext.Object);
            ITokenLogic tokenLogic = new TokenLogic(configuration);
            UserController SUT = new UserController(userLogic, tokenLogic);

            // Act
            IActionResult result = SUT.GetMatchHistoryByUserId(userId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

    }
}

