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
        public void EloCalculation_LowerEloUserWinsAgainstHigherEloUser_ShouldGainMore1v1Elo()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id, elo_1v1, elo_2v2) VALUES (1, 1200, 0), (2, 800, 0)");
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

        [Fact]
        public void EloCalculation_HigherEloUserWinsAgainstLowerEloUser_ShouldGainLess1v1Elo()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id, elo_1v1, elo_2v2) VALUES (1, 800, 0), (2, 1200, 0)");
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
            Assert.True(user1.Elo1v1 < 800);
            Assert.True(user2.Elo1v1 > 1200);
            
            // Calculate expected ELO gain
            const int kFactor = 32;
            double expectedScore = 1.0 / (1.0 + Math.Pow(10, (800 - 1200) / 400.0));
            int expectedEloGain = (int)(kFactor * (1 - expectedScore));
            
            // Ensure the ELO gain for the lower ELO player is significant
            int eloGain = user2.Elo1v1 - 1200;
            Assert.True(expectedEloGain == eloGain);
        }

        [Fact]
        public void EloCalculation_LowerEloUsersWinsAgainstHigherEloUsers_ShouldGainMore2v2Elo()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id, elo_1v1, elo_2v2) VALUES (1, 0, 1200), (2, 0, 1200), (3, 0, 800), (4, 0, 800)");
            _dbHelper.InsertData("INSERT INTO teams (id, player1_id, player2_id) VALUES (1, 1, 2), (2, 3, 4)");
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
            UserModel user3 = userLogic.GetUserById(3);
            UserModel user4 = userLogic.GetUserById(4);
            
            // Assuming the blue team won, their ELOs should increase and red team's ElOs should decrease
            Assert.True(user1.Elo2v2 < 1200);
            Assert.True(user2.Elo2v2 < 1200);
            Assert.True(user3.Elo2v2 > 800);
            Assert.True(user4.Elo2v2 > 800);
            
            // Calculate expected ELO gain
            const int kFactor = 32;
            double expectedScore = 1.0 / (1.0 + Math.Pow(10, (1200 - 800) / 400.0));
            int expectedEloGain = (int)(kFactor * (1 - expectedScore));
            
            // Ensure the ELO gain for the lower ELO player is significant
            int eloGain = ((user3.Elo2v2 + user4.Elo2v2) / 2) - 800;
            Assert.True(expectedEloGain == eloGain);
        }
        
        [Fact]
        public void EloCalculation_HigherEloUsersWinsAgainstLowerEloUsers_ShouldGainLess2v2Elo()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id, elo_1v1, elo_2v2) VALUES (1, 0, 1200), (2, 0, 1200), (3, 0, 800), (4, 0, 800)");
            _dbHelper.InsertData("INSERT INTO teams (id, player1_id, player2_id) VALUES (1, 1, 2), (2, 3, 4)");
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");
            _dbHelper.InsertData("INSERT INTO foosball_matches (id, table_id, red_team_id, blue_team_id, team_red_score, team_blue_score) VALUES (1, 1, 1, 2, 9, 0)");
            _dbHelper.UpdateData("UPDATE foosball_tables SET active_match_id = 1 WHERE id = 1");

            int mockTableId = 1;
            RegisterGoalRequest mockRegisterGoalRequestRedSide = new RegisterGoalRequest
                { TableId = mockTableId, Side = "red" };

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
            IActionResult result = SUT.RegisterGoal(mockRegisterGoalRequestRedSide);
            
            // Assert
            Assert.IsType<OkObjectResult>(result);
            
            // Verify ELO updates
            UserModel user1 = userLogic.GetUserById(1);
            UserModel user2 = userLogic.GetUserById(2);
            UserModel user3 = userLogic.GetUserById(3);
            UserModel user4 = userLogic.GetUserById(4);
            
            // Assuming the blue team won, their ELOs should increase and red team's ElOs should decrease
            Assert.True(user1.Elo2v2 > 1200);
            Assert.True(user2.Elo2v2 > 1200);
            Assert.True(user3.Elo2v2 < 800);
            Assert.True(user4.Elo2v2 < 800);
            
            // Calculate expected ELO gain
            const int kFactor = 32;
            double expectedScore = 1.0 / (1.0 + Math.Pow(10, (800 - 1200) / 400.0));
            int expectedEloGain = (int)(kFactor * (1 - expectedScore));
            
            // Ensure the ELO gain for the lower ELO player is significant
            int eloGain = ((user1.Elo2v2 + user2.Elo2v2) / 2) - 1200;
            Assert.True(expectedEloGain == eloGain);
        }

        [Fact]
        public void EloCalculation_2v1Game_ShouldNotUpdateElo()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id, elo_1v1, elo_2v2) VALUES (1, 1000, 1000), (2, 1000, 1000), (3, 1000, 1000)");
            _dbHelper.InsertData("INSERT INTO teams (id, player1_id, player2_id) VALUES (1, 1, 2), (2, 3, NULL)");
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
            
            // Verify ELO updates
            UserModel user1 = userLogic.GetUserById(1);
            UserModel user2 = userLogic.GetUserById(2);
            UserModel user3 = userLogic.GetUserById(3);
            
            // ELO should not be updated
            Assert.True(user1.Elo2v2 == 1000 && user1.Elo1v1 == 1000);
            Assert.True(user2.Elo2v2 == 1000 && user2.Elo1v1 == 1000);
            Assert.True(user3.Elo2v2 == 1000 && user3.Elo1v1 == 1000);
        }

        [Fact]
        public void EloCalculation_PlayerJoins1v1MidMatchMakingIt2v1_ShouldNotUpdateElo()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id, elo_1v1, elo_2v2) VALUES (1, 1000, 1000), (2, 1000, 1000), (3, 1000, 1000)");
            _dbHelper.InsertData("INSERT INTO teams (id, player1_id) VALUES (1, 1), (2, 2)");
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
            
            // Simulate player joining mid-match
            TableLoginRequest mockTableLoginRequestPlayer3 = new TableLoginRequest
                { UserId = 3, TableId = mockTableId, Side = "red" };
            SUT.LoginOnTable(mockTableLoginRequestPlayer3);
            
            // Act
            IActionResult result = SUT.RegisterGoal(mockRegisterGoalRequestRedSide);
            
            // Assert
            Assert.IsType<OkObjectResult>(result);
            
            // Verify ELO updates
            UserModel user1 = userLogic.GetUserById(1);
            UserModel user2 = userLogic.GetUserById(2);
            UserModel user3 = userLogic.GetUserById(3);

            // ELO should not be updated
            Assert.True(user1.Elo1v1 == 1000 && user1.Elo2v2 == 1000);
            Assert.True(user2.Elo1v1 == 1000 && user2.Elo2v2 == 1000);
            Assert.True(user3.Elo1v1 == 1000 && user3.Elo2v2 == 1000);
        }

        [Fact]
        public void EloCalculation_PlayerJoins2v1MidMatchMakingIt2v2_ShouldNotUpdateElo()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id, elo_1v1, elo_2v2) VALUES (1, 1000, 1000), (2, 1000, 1000), (3, 1000, 1000), (4, 1000, 1000)");
            _dbHelper.InsertData("INSERT INTO teams (id, player1_id, player2_id) VALUES (1, 1, 3), (2, 2, null)");
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
            
            // Simulate player joining mid-match
            TableLoginRequest mockTableLoginRequestPlayer4 = new TableLoginRequest
                { UserId = 4, TableId = mockTableId, Side = "blue" };
            SUT.LoginOnTable(mockTableLoginRequestPlayer4);
            
            // Act
            IActionResult result = SUT.RegisterGoal(mockRegisterGoalRequestRedSide);
            
            // Assert
            Assert.IsType<OkObjectResult>(result);
            
            // Verify ELO updates
            UserModel user1 = userLogic.GetUserById(1);
            UserModel user2 = userLogic.GetUserById(2);
            UserModel user3 = userLogic.GetUserById(3);
            UserModel user4 = userLogic.GetUserById(4);

            // ELO should not be updated
            Assert.True(user1.Elo1v1 == 1000 && user1.Elo2v2 == 1000);
            Assert.True(user2.Elo1v1 == 1000 && user2.Elo2v2 == 1000);
            Assert.True(user3.Elo1v1 == 1000 && user3.Elo2v2 == 1000);
            Assert.True(user4.Elo1v1 == 1000 && user4.Elo2v2 == 1000);
        }

        [Fact]
        public void EloCalculation_EdgeCaseWhereTwoPlayersFromPrevious2v1GameTryToMakeNewTeam_ShouldReturnSucces()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id, elo_1v1, elo_2v2) VALUES (1, 1000, 1000), (2, 1000, 1000), (3, 1000, 1000)");
            _dbHelper.InsertData("INSERT INTO teams (player1_id, player2_id) VALUES (1, 3), (2, null)");
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");
            _dbHelper.InsertData("INSERT INTO foosball_matches (table_id, red_team_id, blue_team_id, team_red_score, team_blue_score) VALUES (1, 1, 2, 9, 0)");
            _dbHelper.UpdateData("UPDATE foosball_tables SET active_match_id = 1 WHERE id = 1");
            
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
            // Interrupt the match
            SUT.InterruptMatch(1);
            
            // Log in users 1 and 2
            TableLoginRequest loginRequestUser1 = new TableLoginRequest { UserId = 1, TableId = 1, Side = "red" };
            TableLoginRequest loginRequestUser2 = new TableLoginRequest { UserId = 2, TableId = 1, Side = "blue" };
            SUT.LoginOnTable(loginRequestUser1);
            SUT.LoginOnTable(loginRequestUser2);
            
            // Start a new match
            IActionResult startMatchResult = SUT.StartMatch(1);
            
            // Assert
            Assert.IsType<OkObjectResult>(startMatchResult);
            
            // Verify the database state
            TeamModel redTeam = matchDatabaseAccessor.GetTeamById(3);
            TeamModel blueTeam = matchDatabaseAccessor.GetTeamById(2);
            
            // Check if user 3 is still part of the team or if only users 1 and 2 are persisted
            Assert.NotNull(redTeam.User1);
            Assert.NotNull(blueTeam.User1);
            Assert.Null(redTeam.User2);
            Assert.Null(blueTeam.User2);
        }
    }
}