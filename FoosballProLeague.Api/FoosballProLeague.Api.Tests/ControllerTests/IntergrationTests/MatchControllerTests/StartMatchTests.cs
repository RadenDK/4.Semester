﻿using FoosballProLeague.Api.BusinessLogic;
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
    public class StartMatchTests : DatabaseTestBase
    {
        [Fact]
        public void StartMatch_WithValidPendingPlayers_ShouldReturnSuccess()
        {
            // Arrange
            int mockTableId = 1;
            int mockPlayer1Id = 1;
            int mockPlayer2Id = 2;

            _dbHelper.InsertData($"INSERT INTO foosball_tables (id) VALUES ({mockTableId})");
            _dbHelper.InsertData($"INSERT INTO users (id) VALUES ({mockPlayer1Id}), ({mockPlayer2Id})");

            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            Mock<IHubContext<MatchHub>> mockHubContext = new Mock<IHubContext<MatchHub>>();
            Mock<IHubClients> mockClients = new Mock<IHubClients>();
            Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();

            mockHubContext.Setup(hub => hub.Clients).Returns(mockClients.Object);
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()));
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, (IHubContext<MatchHub>)mockHubContext.Object, userLogic);
            MatchController SUT = new MatchController(matchLogic);

            TableLoginRequest mockTableLoginRequestPlayer1 = new TableLoginRequest { UserId = mockPlayer1Id, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequestPlayer2 = new TableLoginRequest { UserId = mockPlayer2Id, TableId = mockTableId, Side = "blue" };

            // Act
            SUT.LoginOnTable(mockTableLoginRequestPlayer1);
            SUT.LoginOnTable(mockTableLoginRequestPlayer2);

            IActionResult result = SUT.StartMatch(mockTableId);

            // Assert

            IEnumerable<MatchModel> matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables");
            IEnumerable<TeamModel> teams = _dbHelper.ReadData<TeamModel>("SELECT * FROM teams");

            Assert.IsType<OkObjectResult>(result);

            Assert.True(matches.First() != null);
            Assert.True(matches.Count() == 1);
            Assert.True(matches.First().RedTeamId == 1 && matches.First().BlueTeamId == 2);
            Assert.True(matches.First().TeamRedScore == 0 && matches.First().TeamBlueScore == 0);
            Assert.True(matches.First().StartTime.AddSeconds(5) >= DateTime.Now);
            Assert.True(matches.First().EndTime == null);

            Assert.True(table.First().ActiveMatchId == matches.First().Id);

            Assert.True(teams.Count() == 2);
            Assert.True(teams.First().User1.Id == 1 && teams.First().User2 == null);
            Assert.True(teams.Last().User1.Id == 2 && teams.Last().User2 == null);

            mockClientProxy.Verify(
                client => client.SendCoreAsync(
                    "RecieveMatchStart",
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
        public void StartMatch_WithInvarianceInTeamSize_ShouldReturnSuccess()
        {
            // Arrange
            int mockTableId = 1;
            int mockPlayer1Id = 1;
            int mockPlayer2Id = 2;
            int mockPlayer3Id = 3;

            _dbHelper.InsertData($"INSERT INTO foosball_tables (id) VALUES ({mockTableId})");
            _dbHelper.InsertData($"INSERT INTO users (id) VALUES ({mockPlayer1Id}), ({mockPlayer2Id}), ({mockPlayer3Id})");

            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            Mock<IHubContext<MatchHub>> mockHubContext = new Mock<IHubContext<MatchHub>>();
            Mock<IHubClients> mockClients = new Mock<IHubClients>();
            Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();

            mockHubContext.Setup(hub => hub.Clients).Returns(mockClients.Object);
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()));
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, (IHubContext<MatchHub>)mockHubContext.Object, userLogic);
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

            IEnumerable<MatchModel> matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables");
            IEnumerable<TeamModel> teams = _dbHelper.ReadData<TeamModel>("SELECT * FROM teams");

            Assert.True(matches.First() != null);
            Assert.True(matches.Count() == 1);
            Assert.True(matches.First().RedTeamId == teams.First(t => t.User1.Id == mockPlayer1Id).Id);
            Assert.True(matches.First().BlueTeamId == teams.First(t => t.User1.Id == mockPlayer2Id && t.User2.Id == mockPlayer3Id).Id);
            Assert.True(matches.First().TeamRedScore == 0 && matches.First().TeamBlueScore == 0);
            Assert.True(matches.First().StartTime.AddSeconds(5) >= DateTime.Now);
            Assert.True(matches.First().EndTime == null);

            Assert.True(table.First().ActiveMatchId == matches.First().Id);

            Assert.True(teams.Count() == 2);
            Assert.True(teams.First(t => t.User1.Id == mockPlayer1Id).User2 == null);
            Assert.True(teams.First(t => t.User1.Id == mockPlayer2Id && t.User2.Id == mockPlayer3Id) != null);

            mockClientProxy.Verify(
                client => client.SendCoreAsync(
                    "RecieveMatchStart",
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
        public void StartMatch_WithNoPendingTeams_ShouldReturnBadRequest()
        {
            // Arrange
            int mockTableId = 1;

            _dbHelper.InsertData($"INSERT INTO foosball_tables (id) VALUES ({mockTableId})");

            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            Mock mockHubContext = new Mock<IHubContext<MatchHub>>();
            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()));
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, (IHubContext<MatchHub>)mockHubContext.Object, userLogic);
            MatchController SUT = new MatchController(matchLogic);

            // Act
            IActionResult result = SUT.StartMatch(mockTableId);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);

            IEnumerable<MatchModel> matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables");

            Assert.Empty(matches);
            Assert.Null(table.First().ActiveMatchId);
        }

        [Fact]
        public void StartMatch_ShouldCreateTeamIfNoExistingTeam()
        {
            // Arrange
            int mockTableId = 1;
            int mockPlayer1Id = 1;
            int mockPlayer2Id = 2;

            _dbHelper.InsertData($"INSERT INTO foosball_tables (id) VALUES ({mockTableId})");
            _dbHelper.InsertData($"INSERT INTO users (id) VALUES ({mockPlayer1Id}), ({mockPlayer2Id})");

            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            Mock<IHubContext<MatchHub>> mockHubContext = new Mock<IHubContext<MatchHub>>();
            Mock<IHubClients> mockClients = new Mock<IHubClients>();
            Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();

            mockHubContext.Setup(hub => hub.Clients).Returns(mockClients.Object);
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()));
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, (IHubContext<MatchHub>)mockHubContext.Object, userLogic);
            MatchController SUT = new MatchController(matchLogic);

            TableLoginRequest mockTableLoginRequestPlayer1 = new TableLoginRequest { UserId = mockPlayer1Id, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequestPlayer2 = new TableLoginRequest { UserId = mockPlayer2Id, TableId = mockTableId, Side = "blue" };

            // Act
            SUT.LoginOnTable(mockTableLoginRequestPlayer1);
            SUT.LoginOnTable(mockTableLoginRequestPlayer2);

            IActionResult result = SUT.StartMatch(mockTableId);

            // Assert
            Assert.IsType<OkObjectResult>(result);

            IEnumerable<MatchModel> matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables");
            IEnumerable<TeamModel> teams = _dbHelper.ReadData<TeamModel>("SELECT * FROM teams");
            IEnumerable<UserModel> users = _dbHelper.ReadData<UserModel>("SELECT * FROM users");

            MatchModel createdMatch = matches.FirstOrDefault();
            Assert.NotNull(createdMatch);
            Assert.Equal(createdMatch.Id, table.First().ActiveMatchId);
            Assert.Equal(2, teams.Count());

            TeamModel redTeam = teams.FirstOrDefault(t => t.Id == createdMatch.RedTeamId);
            Assert.NotNull(redTeam);
            Assert.Equal(mockPlayer1Id, redTeam.User1.Id);
            Assert.Null(redTeam.User2);

            TeamModel blueTeam = teams.FirstOrDefault(t => t.Id == createdMatch.BlueTeamId);
            Assert.NotNull(blueTeam);
            Assert.Equal(mockPlayer1Id, blueTeam.User1.Id);
            Assert.Null(blueTeam.User2);

           

            mockClientProxy.Verify(
                client => client.SendCoreAsync(
                    "RecieveMatchStart",
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
        public void StartMatch_ShouldNotCreateTeamIfExistingTeam()
        {
            // Arrange
            int mockTableId = 1;
            int redTeamId = 1;
            int blueTeamId = 2;

            _dbHelper.InsertData($"INSERT INTO foosball_tables (id) VALUES ({mockTableId})");
            _dbHelper.InsertData("INSERT INTO users (id) VALUES (1), (2)");
            _dbHelper.InsertData($"INSERT INTO teams (id, player1_id) VALUES (1, {redTeamId}), (2, {blueTeamId})");

            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            Mock<IHubContext<MatchHub>> mockHubContext = new Mock<IHubContext<MatchHub>>();
            Mock<IHubClients> mockClients = new Mock<IHubClients>();
            Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();

            mockHubContext.Setup(hub => hub.Clients).Returns(mockClients.Object);
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()));
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, (IHubContext<MatchHub>)mockHubContext.Object, userLogic);
            MatchController SUT = new MatchController(matchLogic);

            TableLoginRequest mockTableLoginRequestPlayer1 = new TableLoginRequest { UserId = 1, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequestPlayer2 = new TableLoginRequest { UserId = 2, TableId = mockTableId, Side = "blue" };

            // Act
            SUT.LoginOnTable(mockTableLoginRequestPlayer1);
            SUT.LoginOnTable(mockTableLoginRequestPlayer2);

            IActionResult result = SUT.StartMatch(mockTableId);

            // Assert
            Assert.IsType<OkObjectResult>(result);

            IEnumerable<MatchModel> matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables");
            IEnumerable<TeamModel> teams = _dbHelper.ReadData<TeamModel>("SELECT * FROM teams");

            MatchModel createdMatch = matches.FirstOrDefault();
            Assert.NotNull(createdMatch);
            Assert.Equal(createdMatch.Id, table.First().ActiveMatchId);
            Assert.Equal(2, teams.Count());
            Assert.Equal(redTeamId, createdMatch.RedTeamId);
            Assert.Equal(blueTeamId, createdMatch.BlueTeamId);

            mockClientProxy.Verify(
                client => client.SendCoreAsync(
                    "RecieveMatchStart",
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
        public void StartMatch_WithActiveMatch_ShouldReturnBadRequest()
        {
            // Arrange
            int mockTableId = 1;
            int redTeamId = 1;
            int blueTeamId = 2;
            int matchId = 1;

            _dbHelper.InsertData("INSERT INTO users (id) VALUES (1), (2)");
            _dbHelper.InsertData($"INSERT INTO teams (id, player1_id) VALUES (1, {redTeamId}), (2, {blueTeamId})");
            _dbHelper.InsertData($"INSERT INTO foosball_tables (id) VALUES ({mockTableId})");
            _dbHelper.InsertData($"INSERT INTO foosball_matches (id, table_id, red_team_id, blue_team_id) VALUES ({matchId}, {mockTableId}, {redTeamId}, {blueTeamId})");
            _dbHelper.UpdateData($"UPDATE foosball_tables SET active_match_id = {matchId} WHERE id = {mockTableId}");


            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            Mock mockHubContext = new Mock<IHubContext<MatchHub>>();
            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()));
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, (IHubContext<MatchHub>)mockHubContext.Object, userLogic);
            MatchController SUT = new MatchController(matchLogic);

            TableLoginRequest mockTableLoginRequestPlayer1 = new TableLoginRequest { UserId = 1, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequestPlayer2 = new TableLoginRequest { UserId = 2, TableId = mockTableId, Side = "blue" };

            // Act
            SUT.LoginOnTable(mockTableLoginRequestPlayer1);
            SUT.LoginOnTable(mockTableLoginRequestPlayer2);

            IActionResult result = SUT.StartMatch(mockTableId);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);

            IEnumerable<MatchModel> matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables");

            Assert.Equal(1, matches.Count());
            Assert.Equal(matchId, table.First().ActiveMatchId);
        }

        [Fact]
        public void StartMatch_WithOnlyOneExistingTeam_ShouldCreateOneTeamAndReuseTheExistingOne()
        {
            // Arrange
            int mockTableId = 1;
            int existingTeamId = 1;
            int mockPlayer1Id = 1;
            int mockPlayer2Id = 2;
            int mockPlayer3Id = 3;

            _dbHelper.InsertData($"INSERT INTO foosball_tables (id) VALUES ({mockTableId})");
            _dbHelper.InsertData($"INSERT INTO users (id) VALUES ({mockPlayer1Id}), ({mockPlayer2Id}), ({mockPlayer3Id})");
            _dbHelper.InsertData($"INSERT INTO teams (player1_id) VALUES ({mockPlayer1Id})");

            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            Mock<IHubContext<MatchHub>> mockHubContext = new Mock<IHubContext<MatchHub>>();
            Mock<IHubClients> mockClients = new Mock<IHubClients>();
            Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();

            mockHubContext.Setup(hub => hub.Clients).Returns(mockClients.Object);
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()));
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, (IHubContext<MatchHub>)mockHubContext.Object, userLogic);
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

            IEnumerable<MatchModel> matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables");
            IEnumerable<TeamModel> teams = _dbHelper.ReadData<TeamModel>("SELECT * FROM teams");

            MatchModel createdMatch = matches.FirstOrDefault();
            Assert.NotNull(createdMatch);
            Assert.Equal(mockTableId, createdMatch.TableId);
            Assert.True(table.First().ActiveMatchId == createdMatch.Id);
            Assert.Equal(2, teams.Count());

            TeamModel existingTeam = teams.FirstOrDefault(t => t.Id == existingTeamId);
            Assert.NotNull(existingTeam);

            TeamModel newTeam = teams.FirstOrDefault(t => t.User1.Id == 2 && t.User2.Id == 3);
            Assert.NotNull(newTeam);

            Assert.Equal(existingTeam.Id, createdMatch.RedTeamId);
            Assert.Equal(newTeam.Id, createdMatch.BlueTeamId);

            mockClientProxy.Verify(
                client => client.SendCoreAsync(
                    "RecieveMatchStart",
                    It.Is<object[]>(o => o != null && o.Length == 5 &&
                                    (bool)o[0] == true &&
                                    ((TeamModel)o[1]).Id == 1 &&
                                    ((TeamModel)o[2]).Id == 2 &&
                                    (int)o[3] == 0 &&
                                    (int)o[4] == 0),
                default),
                Times.Once);
        }
    }
}
