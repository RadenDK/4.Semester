using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.Hubs;
using FoosballProLeague.Api.Models;
using FoosballProLeague.Api.Models.FoosballModels;
using FoosballProLeague.Api.Models.RequestModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace FoosballProLeague.Api.Tests.ControllerTests.IntergrationTests.MatchControllerTests
{

    [Collection("Non-Parallel Database Collection")]
    public class EloTests : DatabaseTestBase
    {
        [Fact]
        public void EloCalculation_AfterMatchCompletion_ShouldUpdateEloCorrectly()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id, elo_1v1, elo_2v2) VALUES (1, 1000, 1000), (2, 1000, 1000), (3, 1000, 1000), (4, 1000, 1000)");
            _dbHelper.InsertData("INSERT INTO teams (id, player1_id, player2_id) VALUES (1, 1, 2), (2, 3, 4)");
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");
            _dbHelper.InsertData("INSERT INTO foosball_matches (id, table_id, red_team_id, blue_team_id, team_red_score, team_blue_score) VALUES (1, 1, 1, 2, 9, 0)");
            _dbHelper.UpdateData("UPDATE foosball_tables SET active_match_id = 1 WHERE id = 1");

            int mockTableId = 1;
            RegisterGoalRequest mockRegisterGoalRequestRedSide =
                new RegisterGoalRequest { TableId = mockTableId, Side = "red" };

            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            Mock<IHubContext<MatchHub>> mockHubContext = new Mock<IHubContext<MatchHub>>();
            Mock<IHubClients> mockClients = new Mock<IHubClients>();
            Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();

            mockHubContext.Setup(hub => hub.Clients).Returns(mockClients.Object);
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()));
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, (IHubContext<MatchHub>)mockHubContext.Object, userLogic);
            MatchController SUT = new MatchController(matchLogic);
            
            // Act
            IActionResult result = SUT.RegisterGoal(mockRegisterGoalRequestRedSide);
            
            // Assert
            Assert.IsType<OkObjectResult>(result);
            
            //Verify ELO updates
            UserModel user1 = userLogic.GetUserById(1);
            UserModel user2 = userLogic.GetUserById(2);
            UserModel user3 = userLogic.GetUserById(3);
            UserModel user4 = userLogic.GetUserById(4);
            
            // Assuming the red team won, their ELOs should increase and blue team's ELOs should decrease
            Assert.True(user1.Elo2v2 > 1000);
            Assert.True(user2.Elo2v2 > 1000);
            Assert.True(user3.Elo2v2 < 1000);
            Assert.True(user4.Elo2v2 < 1000);

            mockClientProxy.Verify(
                client => client.SendCoreAsync(
                    "RecieveGoalUpdate",
                    It.Is<object[]>(o => o != null && o.Length == 4 &&
                                    ((TeamModel)o[0]).Id == 1 &&
                                    ((TeamModel)o[1]).Id == 2 &&
                                    (int)o[2] == 10 &&
                                    (int)o[3] == 0),
                default),
                Times.Once);
        }

        [Fact]
        public void EloCalculation_LowerEloPlayerWinsAgainstHigherEloPlayer_ShouldGainMoreElo()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id, elo_1v1, elo_2v2) VALUES (1, 1200, 1200), (2, 800, 800)");
            _dbHelper.InsertData("INSERT INTO teams (id, player1_id) VALUES (1, 1), (2, 2)");
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");
            _dbHelper.InsertData("INSERT INTO foosball_matches (id, table_id, red_team_id, blue_team_id, team_red_score, team_blue_score) VALUES (1, 1, 1, 2, 0, 9)");
            _dbHelper.UpdateData("UPDATE foosball_tables SET active_match_id = 1 WHERE id = 1");

            int mockTableId = 1;
            RegisterGoalRequest mockRegisterGoalRequestBlueSide = new RegisterGoalRequest
                { TableId = mockTableId, Side = "blue" };

            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            Mock<IHubContext<MatchHub>> mockHubContext = new Mock<IHubContext<MatchHub>>();
            Mock<IHubClients> mockClients = new Mock<IHubClients>();
            Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();

            mockHubContext.Setup(hub => hub.Clients).Returns(mockClients.Object);
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()));
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, (IHubContext<MatchHub>)mockHubContext.Object,
                userLogic);
            MatchController SUT = new MatchController(matchLogic);
            
            // Act
            IActionResult result = SUT.RegisterGoal(mockRegisterGoalRequestBlueSide);
            
            // Assert
            Assert.IsType<OkObjectResult>(result);
            
            // Verify ELO updates
            UserModel user1 = userLogic.GetUserById(1);
            UserModel user2 = userLogic.GetUserById(2);
            
            // Assuming the blue team won, their ELOs should increase and red team's ElOs should decrease
            Assert.True(user1.Elo1v1 < 1200);
            Assert.True(user2.Elo1v1 > 800);
            
            // Calculate expected ELO gain
            const int kFactor = 32;
            double expectedScore = 1.0 / (1.0 + Math.Pow(10, (1200 - 800) / 400.0));
            int expectedEloGain = (int)(kFactor * (1 - expectedScore));
            
            // Ensure the ELO gain for the lower ELO player is significant
            int eloGain = user2.Elo1v1 - 800;
            Assert.True(expectedEloGain == eloGain);
        }
    }
}