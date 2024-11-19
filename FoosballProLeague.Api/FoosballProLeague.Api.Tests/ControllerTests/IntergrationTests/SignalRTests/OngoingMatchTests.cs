using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.BusinessLogic.Interfaces;
using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.DatabaseAccess.Interfaces;
using FoosballProLeague.Api.Hubs;
using FoosballProLeague.Api.Models.FoosballModels;
using FoosballProLeague.Api.Models.RequestModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Moq;


namespace FoosballProLeague.Api.Tests.ControllerTests.IntergrationTests.SignalRTests
{
    [Collection("Non-Parallel Database Collection")]
    public class OngoingMatchTests : DatabaseTestBase
    {
        // Start match tests with endpoint "RecieveMatchStart"
        [Fact]
        public void StartMatch_WithValidPendingPlayers_ShouldSendCorrectData()
        {
            // Arrange
            int mockTableId = 1;
            int mockPlayer1Id = 1;
            int mockPlayer2Id = 2;

            _dbHelper.InsertData($"INSERT INTO foosball_tables (id) VALUES ({mockTableId})");
            _dbHelper.InsertData($"INSERT INTO users (id) VALUES ({mockPlayer1Id}), ({mockPlayer2Id})");

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);

            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();
            Mock<IHubClients> mockClients = new Mock<IHubClients>();
            Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();

            mockHubContext.Setup(hub => hub.Clients).Returns(mockClients.Object);
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, mockHubContext.Object, userLogic, teamDatabaseAccessor);
            
            MatchController SUT = new MatchController(matchLogic);

            TableLoginRequest mockTableLoginRequestPlayer1 = new TableLoginRequest { UserId = mockPlayer1Id, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequestPlayer2 = new TableLoginRequest { UserId = mockPlayer2Id, TableId = mockTableId, Side = "blue" };

            // Act
            SUT.LoginOnTable(mockTableLoginRequestPlayer1);
            SUT.LoginOnTable(mockTableLoginRequestPlayer2);

            IActionResult result = SUT.StartMatch(mockTableId);

            // Assert
            mockClientProxy.Verify(
                client => client.SendCoreAsync(
                    "ReceiveMatchStart",
                    It.Is<object[]>(o => o != null && o.Length == 5 &&
                                    (bool)o[0] == true &&
                                    ((TeamModel)o[1]).Id == 1 &&
                                    ((TeamModel)o[2]).Id == 2 &&
                                    (int)o[3] == 0 &&
                                    (int)o[4] == 0),
                default),
                Times.Once);
        }

        [Fact]
        public void StartMatch_WithInvarianceInTeamSize_ShouldSendCorrectData()
        {
            // Arrange
            int mockTableId = 1;
            int mockPlayer1Id = 1;
            int mockPlayer2Id = 2;
            int mockPlayer3Id = 3;

            _dbHelper.InsertData($"INSERT INTO foosball_tables (id) VALUES ({mockTableId})");
            _dbHelper.InsertData($"INSERT INTO users (id) VALUES ({mockPlayer1Id}), ({mockPlayer2Id}), ({mockPlayer3Id})");

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);

            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();
            Mock<IHubClients> mockClients = new Mock<IHubClients>();
            Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();

            mockHubContext.Setup(hub => hub.Clients).Returns(mockClients.Object);
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, mockHubContext.Object, userLogic, teamDatabaseAccessor);

            MatchController SUT = new MatchController(matchLogic);

            TableLoginRequest mockTableLoginRequestPlayer1 = new TableLoginRequest { UserId = mockPlayer1Id, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequestPlayer2 = new TableLoginRequest { UserId = mockPlayer2Id, TableId = mockTableId, Side = "blue" };
            TableLoginRequest mockTableLoginRequestPlayer3 = new TableLoginRequest { UserId = mockPlayer3Id, TableId = mockTableId, Side = "blue" };

            // Act
            SUT.LoginOnTable(mockTableLoginRequestPlayer1);
            SUT.LoginOnTable(mockTableLoginRequestPlayer2);
            SUT.LoginOnTable(mockTableLoginRequestPlayer3);

            IActionResult result = SUT.StartMatch(mockTableId);

            // Assert
            Assert.IsType<OkObjectResult>(result);

            mockClientProxy.Verify(
                client => client.SendCoreAsync(
                    "ReceiveMatchStart",
                    It.Is<object[]>(o => o != null && o.Length == 5 &&
                                    (bool)o[0] == true &&
                                    ((TeamModel)o[1]).Id == 1 &&
                                    ((TeamModel)o[2]).Id == 2 &&
                                    (int)o[3] == 0 &&
                                    (int)o[4] == 0),
                default),
                Times.Once);
        }


        // Register Goal tests with endpoint "RecieveGoalUpdate"
        [Fact]
        public void GoalScored_OnActiveTable_ShouldSendCorrectMatchInfo()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id) VALUES (1), (2)");
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");
            _dbHelper.InsertData("INSERT INTO teams (id, player1_id) VALUES (1, 1), (2, 2)");
            _dbHelper.InsertData("INSERT INTO foosball_matches (id, table_id, red_team_id, blue_team_id, team_red_score, team_blue_score) VALUES (1, 1, 1, 2, 4, 0)"); // Active match where red has scored 4 goals and blue 0
            _dbHelper.UpdateData("UPDATE foosball_tables SET active_match_id = 1 WHERE id = 1");

            int mockTableId = 1;

            RegisterGoalRequest mockRegisterGoalRequest = new RegisterGoalRequest { TableId = mockTableId, Side = "red" };

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);

            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();
            Mock<IHubClients> mockClients = new Mock<IHubClients>();
            Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();

            mockHubContext.Setup(hub => hub.Clients).Returns(mockClients.Object);
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, mockHubContext.Object, userLogic, teamDatabaseAccessor);

            MatchController SUT = new MatchController(matchLogic);

            // Act
            IActionResult result = SUT.RegisterGoal(mockRegisterGoalRequest);

            // Assert
            mockClientProxy.Verify(
                client => client.SendCoreAsync(
                    "ReceiveGoalUpdate",
                    It.Is<object[]>(o => o != null && o.Length == 4 &&
                                    ((TeamModel)o[0]).Id == 1 &&
                                    ((TeamModel)o[1]).Id == 2 &&
                                    (int)o[2] == 5 &&
                                    (int)o[3] == 0),
                default),
                Times.Once);
        }


        [Fact]
        public void GoalScored_WithOngoingMatch_ShouldSendCorrectMatchInfo()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id) VALUES (1), (2)");
            _dbHelper.InsertData("INSERT INTO teams (id, player1_id) VALUES (1, 1), (2, 2)");
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");
            _dbHelper.InsertData("INSERT INTO foosball_matches (id, table_id, red_team_id, blue_team_id, team_red_score, team_blue_score) VALUES (1, 1, 1, 2, 4, 0)"); // Active match where red has scored 4 goals and blue 0
            _dbHelper.UpdateData("UPDATE foosball_tables SET active_match_id = 1 WHERE id = 1");

            int mockTableId = 1;

            RegisterGoalRequest mockRegisterGoalRequest = new RegisterGoalRequest { TableId = mockTableId, Side = "red" };

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);

            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();
            Mock<IHubClients> mockClients = new Mock<IHubClients>();
            Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();

            mockHubContext.Setup(hub => hub.Clients).Returns(mockClients.Object);
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, mockHubContext.Object, userLogic, teamDatabaseAccessor);

            MatchController SUT = new MatchController(matchLogic);

            // Act
            IActionResult result = SUT.RegisterGoal(mockRegisterGoalRequest);

            // Assert
            IEnumerable<MatchModel> matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables");
            IEnumerable<MatchLogModel> matchLogs = _dbHelper.ReadData<MatchLogModel>("SELECT * FROM match_logs");

            mockClientProxy.Verify(
                client => client.SendCoreAsync(
                    "ReceiveGoalUpdate",
                    It.Is<object[]>(o => o != null && o.Length == 4 &&
                                    ((TeamModel)o[0]).Id == 1 &&
                                    ((TeamModel)o[1]).Id == 2 &&
                                    (int)o[2] == 5 &&
                                    (int)o[3] == 0),
                default),
                Times.Once);
        }


        [Fact]
        public void GoalScored_WithUnbalancedTeams_ShouldSendCorrectMatchInfo()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id) VALUES (1), (2), (3)");
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");
            _dbHelper.InsertData("INSERT INTO teams (id, player1_id) VALUES (1, 1)");
            _dbHelper.InsertData("INSERT INTO teams (id, player1_id, player2_id) VALUES (2, 2, 3)"); // Blue team has 2 players
            _dbHelper.InsertData("INSERT INTO foosball_matches (id, table_id, red_team_id, blue_team_id, team_red_score, team_blue_score) VALUES (1, 1, 1, 2, 0, 0)"); // Active match where red has scored 0 goals and blue 0
            _dbHelper.UpdateData("UPDATE foosball_tables SET active_match_id = 1 WHERE id = 1");

            int mockTableId = 1;

            RegisterGoalRequest mockRegisterGoalRequest = new RegisterGoalRequest { TableId = mockTableId, Side = "red" };

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);

            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();
            Mock<IHubClients> mockClients = new Mock<IHubClients>();
            Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();

            mockHubContext.Setup(hub => hub.Clients).Returns(mockClients.Object);
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, mockHubContext.Object, userLogic, teamDatabaseAccessor);

            MatchController SUT = new MatchController(matchLogic);

            // Act
            IActionResult result = SUT.RegisterGoal(mockRegisterGoalRequest);

            // Assert
            mockClientProxy.Verify(
                client => client.SendCoreAsync(
                    "ReceiveGoalUpdate",
                    It.Is<object[]>(o => o != null && o.Length == 4 &&
                                    ((TeamModel)o[0]).Id == 1 &&
                                    ((TeamModel)o[1]).Id == 2 &&
                                    (int)o[2] == 1 &&
                                    (int)o[3] == 0),
                default),
                Times.Once);
        }

        // Register Goal tests with endpoint "RecieveMatchEnd"
        [Fact]
        public void GoalScored_WhenSideReaches10Goals_ShouldSendCorrectMatchInfo()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id) VALUES (1), (2)");
            _dbHelper.InsertData("INSERT INTO teams (id, player1_id) VALUES (1, 1), (2, 2)");
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");
            _dbHelper.InsertData("INSERT INTO foosball_matches (id, table_id, red_team_id, blue_team_id, team_red_score, team_blue_score) VALUES (1, 1, 1, 2, 9, 0)"); // Active match where red has scored 9 goals and blue 0
            _dbHelper.UpdateData("UPDATE foosball_tables SET active_match_id = 1 WHERE id = 1");

            int mockTableId = 1;

            RegisterGoalRequest mockRegisterGoalRequestRedSide = new RegisterGoalRequest { TableId = mockTableId, Side = "red" };

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);

            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();
            Mock<IHubClients> mockClients = new Mock<IHubClients>();
            Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();

            mockHubContext.Setup(hub => hub.Clients).Returns(mockClients.Object);
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, mockHubContext.Object, userLogic, teamDatabaseAccessor);

            MatchController SUT = new MatchController(matchLogic);

            // Act
            IActionResult result = SUT.RegisterGoal(mockRegisterGoalRequestRedSide);

            // Assert
            mockClientProxy.Verify(
                client => client.SendCoreAsync(
                    "ReceiveMatchEnd",
                    It.Is<object[]>(o => o != null && o.Length == 1 &&
                                    (bool)o[0] == false),
                default),
                Times.Once);
        }
    }
}

