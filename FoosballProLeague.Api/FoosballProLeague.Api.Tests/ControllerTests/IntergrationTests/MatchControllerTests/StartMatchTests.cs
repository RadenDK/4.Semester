using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.BusinessLogic.Interfaces;
using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.DatabaseAccess.Interfaces;
using FoosballProLeague.Api.Hubs;
using FoosballProLeague.Api.Models;
using FoosballProLeague.Api.Models.DbModels;
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
        public void StartMatch_WithValidPendingUsers_ShouldReturnSuccess()
        {
            // Arrange
            int mockTableId = 1;
            int mockUser1Id = 1;
            int mockUser2Id = 2;

            _dbHelper.InsertData($"INSERT INTO foosball_tables (id) VALUES ({mockTableId})");
            _dbHelper.InsertData($"INSERT INTO users (id) VALUES ({mockUser1Id}), ({mockUser2Id})");

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);
            Mock<IHubContext<HomepageHub>> mockHomepageHubContext = new Mock<IHubContext<HomepageHub>>();
            Mock<IHubContext<TableLoginHub>> mockTableLoginHubContext = new Mock<IHubContext<TableLoginHub>>();

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHomepageHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, mockHomepageHubContext.Object, mockTableLoginHubContext.Object, userLogic, teamDatabaseAccessor);

            MatchController SUT = new MatchController(matchLogic);

            TableLoginRequest mockTableLoginRequestUser1 = new TableLoginRequest { UserId = mockUser1Id, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequestUser2 = new TableLoginRequest { UserId = mockUser2Id, TableId = mockTableId, Side = "blue" };

            // Act
            SUT.LoginOnTable(mockTableLoginRequestUser1);
            SUT.LoginOnTable(mockTableLoginRequestUser2);

            IActionResult result = SUT.StartMatch(mockTableId);

            // Assert

            Assert.IsType<OkObjectResult>(result);

            IEnumerable<MatchDbModel> dbMatches = _dbHelper.ReadData<MatchDbModel>("SELECT * FROM foosball_matches");

            Assert.True(dbMatches.Count() == 1);

            MatchDbModel dbMatch = dbMatches.First();

            Assert.True(dbMatch.RedTeamId == 1 && dbMatch.BlueTeamId == 2);
            Assert.True(dbMatch.TeamRedScore == 0 && dbMatch.TeamBlueScore == 0);
            Assert.True(dbMatch.StartTime.AddSeconds(5) >= DateTime.Now);
            Assert.True(dbMatch.EndTime == null);

            FoosballTableModel table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables WHERE id = " + dbMatch.TableId).First();

            Assert.True(table.ActiveMatchId == dbMatch.Id);

            IEnumerable<TeamDbModel> dbTeams = _dbHelper.ReadData<TeamDbModel>("SELECT * FROM teams");

            Assert.True(dbTeams.Count() == 2);

            TeamDbModel dbRedTeam = dbTeams.Where(t => t.Id == dbMatch.RedTeamId).First();
            TeamDbModel dbBlueTeam = dbTeams.Where(t => t.Id == dbMatch.BlueTeamId).First();

            Assert.True(dbRedTeam.User1Id == mockUser1Id && dbRedTeam.User2Id == null);
            Assert.True(dbBlueTeam.User1Id == mockUser2Id && dbBlueTeam.User2Id == null);
        }

        [Fact]
        public void StartMatch_WithInvarianceInTeamSize_ShouldReturnSuccess()
        {
            // Arrange
            int mockTableId = 1;
            int mockUser1Id = 1;
            int mockUser2Id = 2;
            int mockUser3Id = 3;

            _dbHelper.InsertData($"INSERT INTO foosball_tables (id) VALUES ({mockTableId})");
            _dbHelper.InsertData($"INSERT INTO users (id) VALUES ({mockUser1Id}), ({mockUser2Id}), ({mockUser3Id})");

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);
            Mock<IHubContext<HomepageHub>> mockHomepageHubContext = new Mock<IHubContext<HomepageHub>>();
            Mock<IHubContext<TableLoginHub>> mockTableLoginHubContext = new Mock<IHubContext<TableLoginHub>>();

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHomepageHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, mockHomepageHubContext.Object, mockTableLoginHubContext.Object, userLogic, teamDatabaseAccessor);

            MatchController SUT = new MatchController(matchLogic);

            TableLoginRequest mockTableLoginRequestUser1 = new TableLoginRequest { UserId = mockUser1Id, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequestUser2 = new TableLoginRequest { UserId = mockUser2Id, TableId = mockTableId, Side = "blue" };
            TableLoginRequest mockTableLoginRequestUser3 = new TableLoginRequest { UserId = mockUser3Id, TableId = mockTableId, Side = "blue" };

            // Act
            SUT.LoginOnTable(mockTableLoginRequestUser1);
            SUT.LoginOnTable(mockTableLoginRequestUser2);
            SUT.LoginOnTable(mockTableLoginRequestUser3);

            IActionResult result = SUT.StartMatch(mockTableId);

            // Assert
            Assert.IsType<OkObjectResult>(result);

            IEnumerable<MatchDbModel> dbMatches = _dbHelper.ReadData<MatchDbModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> dbTables = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables");
            IEnumerable<TeamDbModel> dbTeams = _dbHelper.ReadData<TeamDbModel>("SELECT * FROM teams");

            Assert.True(dbMatches.Count() == 1);

            MatchDbModel dbMatch = dbMatches.First();

            Assert.True(dbMatch != null);
            Assert.True(dbMatch.TeamRedScore == 0 && dbMatch.TeamBlueScore == 0);
            Assert.True(dbMatch.StartTime.AddSeconds(5) >= DateTime.Now);
            Assert.True(dbMatch.EndTime == null);


            Assert.True(dbTeams.Count() == 2);

            TeamDbModel dbRedTeam = dbTeams.FirstOrDefault(t => t.Id == dbMatch.RedTeamId);
            TeamDbModel dbBlueTeam = dbTeams.FirstOrDefault(t => t.Id == dbMatch.BlueTeamId);

            Assert.True(mockUser1Id == dbRedTeam.User1Id && dbRedTeam.User2Id == null);
            Assert.True(mockUser2Id == dbBlueTeam.User1Id && dbBlueTeam.User2Id == mockUser3Id);

            Assert.True(dbTables.First().ActiveMatchId == dbMatch.Id);
        }

        [Fact]
        public void StartMatch_WithNoPendingTeams_ShouldReturnBadRequest()
        {
            // Arrange
            int mockTableId = 1;

            _dbHelper.InsertData($"INSERT INTO foosball_tables (id) VALUES ({mockTableId})");

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);
            Mock<IHubContext<HomepageHub>> mockHomepageHubContext = new Mock<IHubContext<HomepageHub>>();
            Mock<IHubContext<TableLoginHub>> mockTableLoginHubContext = new Mock<IHubContext<TableLoginHub>>();

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHomepageHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, mockHomepageHubContext.Object, mockTableLoginHubContext.Object, userLogic, teamDatabaseAccessor);

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
        public void StartMatch_ShouldCreateTeamsAndReturnSuccessIfNoTeamsExistingPrior()
        {
            // Arrange
            int mockTableId = 1;
            int mockUser1Id = 1;
            int mockUser2Id = 2;

            _dbHelper.InsertData($"INSERT INTO foosball_tables (id) VALUES ({mockTableId})");
            _dbHelper.InsertData($"INSERT INTO users (id) VALUES ({mockUser1Id}), ({mockUser2Id})");

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);
            Mock<IHubContext<HomepageHub>> mockHomepageHubContext = new Mock<IHubContext<HomepageHub>>();
            Mock<IHubContext<TableLoginHub>> mockTableLoginHubContext = new Mock<IHubContext<TableLoginHub>>();

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHomepageHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, mockHomepageHubContext.Object, mockTableLoginHubContext.Object, userLogic, teamDatabaseAccessor);

            MatchController SUT = new MatchController(matchLogic);

            TableLoginRequest mockTableLoginRequestUser1 = new TableLoginRequest { UserId = mockUser1Id, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequestUser2 = new TableLoginRequest { UserId = mockUser2Id, TableId = mockTableId, Side = "blue" };

            // Act
            SUT.LoginOnTable(mockTableLoginRequestUser1);
            SUT.LoginOnTable(mockTableLoginRequestUser2);
            IActionResult result = SUT.StartMatch(mockTableId);

            // Assert
            Assert.IsType<OkObjectResult>(result);

            // Verify match creation in the database
            IEnumerable<MatchDbModel> dbMatches = _dbHelper.ReadData<MatchDbModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> dbTables = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables");

            Assert.True(dbMatches.Count() == 1);
            
            MatchDbModel dbMatch = dbMatches.FirstOrDefault();
            
            Assert.True(dbMatch.TeamRedScore == 0);
            Assert.True(dbMatch.TeamBlueScore == 0);

            var test = DateTime.Now;
            var test1 = dbMatch.StartTime.AddSeconds(5);

            Assert.True(dbMatch.StartTime.AddSeconds(5) >= DateTime.Now);
            Assert.True(dbMatch.EndTime == null);

            // Verify that the match ID is assigned to the table
            Assert.True(dbTables.Count() == 1);

            FoosballTableModel dbTable = dbTables.First();

            Assert.Equal(dbMatch.Id, dbTable.ActiveMatchId);

            // Verify team creation in the database
            IEnumerable<TeamDbModel> teams = _dbHelper.ReadData<TeamDbModel>("SELECT * FROM teams");
            Assert.Equal(2, teams.Count());

            // Verify red and blue teams are created with correct user IDs
            TeamDbModel dbRedTeam = teams.FirstOrDefault(t => t.Id == dbMatch.RedTeamId);
            TeamDbModel dbBlueTeam = teams.FirstOrDefault(t => t.Id == dbMatch.BlueTeamId);

            // Check that the red team has mockUser1Id and no second user
            Assert.True(dbRedTeam.User1Id == mockUser1Id && dbRedTeam.User2Id == null );

            // Check that the blue team has mockUser2Id and no second user
            Assert.True(dbBlueTeam.User1Id == mockUser2Id && dbBlueTeam.User2Id == null );
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
            _dbHelper.InsertData($"INSERT INTO teams (id, user1_id) VALUES (1, {redTeamId}), (2, {blueTeamId})");

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);
            Mock<IHubContext<HomepageHub>> mockHomepageHubContext = new Mock<IHubContext<HomepageHub>>();
            Mock<IHubContext<TableLoginHub>> mockTableLoginHubContext = new Mock<IHubContext<TableLoginHub>>();

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHomepageHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, mockHomepageHubContext.Object, mockTableLoginHubContext.Object, userLogic, teamDatabaseAccessor);

            MatchController SUT = new MatchController(matchLogic);

            TableLoginRequest mockTableLoginRequestUser1 = new TableLoginRequest { UserId = 1, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequestUser2 = new TableLoginRequest { UserId = 2, TableId = mockTableId, Side = "blue" };

            // Act
            SUT.LoginOnTable(mockTableLoginRequestUser1);
            SUT.LoginOnTable(mockTableLoginRequestUser2);

            IActionResult result = SUT.StartMatch(mockTableId);

            // Assert
            Assert.IsType<OkObjectResult>(result);

            IEnumerable<MatchDbModel> dbMatches = _dbHelper.ReadData<MatchDbModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> dbTable = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables");
            IEnumerable<TeamDbModel> dbTeams = _dbHelper.ReadData<TeamDbModel>("SELECT * FROM teams");

            Assert.True(dbMatches.Count() == 1);


            MatchDbModel dbMatch = dbMatches.First();

            Assert.True(dbMatch.Id == dbTable.First().ActiveMatchId);
            
            Assert.True(dbTeams.Count() == 2);
            Assert.True(redTeamId == dbMatch.RedTeamId);
            Assert.True(blueTeamId == dbMatch.BlueTeamId);

            Assert.True(dbTeams.Count() == 2);

            TeamDbModel redTeam = dbTeams.Where(dbTeams => dbTeams.Id == redTeamId).FirstOrDefault();
            TeamDbModel blueTeam = dbTeams.Where(dbTeams => dbTeams.Id == blueTeamId).FirstOrDefault();

            Assert.True(redTeam.User1Id == 1 && redTeam.User2Id == null);
            Assert.True(blueTeam.User1Id == 2 && blueTeam.User2Id == null);
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
            _dbHelper.InsertData($"INSERT INTO teams (id, user1_id) VALUES (1, {redTeamId}), (2, {blueTeamId})");
            _dbHelper.InsertData($"INSERT INTO foosball_tables (id) VALUES ({mockTableId})");
            _dbHelper.InsertData($"INSERT INTO foosball_matches (id, table_id, red_team_id, blue_team_id) VALUES ({matchId}, {mockTableId}, {redTeamId}, {blueTeamId})");
            _dbHelper.UpdateData($"UPDATE foosball_tables SET active_match_id = {matchId} WHERE id = {mockTableId}");


            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);
            Mock<IHubContext<HomepageHub>> mockHomepageHubContext = new Mock<IHubContext<HomepageHub>>();
            Mock<IHubContext<TableLoginHub>> mockTableLoginHubContext = new Mock<IHubContext<TableLoginHub>>();

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHomepageHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, mockHomepageHubContext.Object, mockTableLoginHubContext.Object, userLogic, teamDatabaseAccessor);

            MatchController SUT = new MatchController(matchLogic);

            TableLoginRequest mockTableLoginRequestUser1 = new TableLoginRequest { UserId = 1, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequestUser2 = new TableLoginRequest { UserId = 2, TableId = mockTableId, Side = "blue" };

            // Act
            SUT.LoginOnTable(mockTableLoginRequestUser1);
            SUT.LoginOnTable(mockTableLoginRequestUser2);

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
            int mockUser1Id = 1;
            int mockUser2Id = 2;
            int mockUser3Id = 3;

            _dbHelper.InsertData($"INSERT INTO foosball_tables (id) VALUES ({mockTableId})");
            _dbHelper.InsertData($"INSERT INTO users (id) VALUES ({mockUser1Id}), ({mockUser2Id}), ({mockUser3Id})");
            _dbHelper.InsertData($"INSERT INTO teams (user1_id) VALUES ({mockUser1Id})");

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);
            Mock<IHubContext<HomepageHub>> mockHomepageHubContext = new Mock<IHubContext<HomepageHub>>();
            Mock<IHubContext<TableLoginHub>> mockTableLoginHubContext = new Mock<IHubContext<TableLoginHub>>();

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHomepageHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, mockHomepageHubContext.Object, mockTableLoginHubContext.Object, userLogic, teamDatabaseAccessor);

            MatchController SUT = new MatchController(matchLogic);

            TableLoginRequest mockTableLoginRequestUser1 = new TableLoginRequest { UserId = mockUser1Id, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequestUser2 = new TableLoginRequest { UserId = mockUser2Id, TableId = mockTableId, Side = "blue" };
            TableLoginRequest mockTableLoginRequestUser3 = new TableLoginRequest { UserId = mockUser3Id, TableId = mockTableId, Side = "blue" };

            // Act
            SUT.LoginOnTable(mockTableLoginRequestUser1);
            SUT.LoginOnTable(mockTableLoginRequestUser2);
            SUT.LoginOnTable(mockTableLoginRequestUser3);

            IActionResult result = SUT.StartMatch(mockTableId);

            // Assert
            Assert.IsType<OkObjectResult>(result);

            // Verify match creation in the database
            IEnumerable<MatchDbModel> dbMatches = _dbHelper.ReadData<MatchDbModel>("SELECT * FROM foosball_matches");
            MatchDbModel createdMatch = dbMatches.FirstOrDefault();
            Assert.NotNull(createdMatch);
            Assert.Equal(mockTableId, createdMatch.TableId);
            Assert.Equal(0, createdMatch.TeamRedScore);
            Assert.Equal(0, createdMatch.TeamBlueScore);
            Assert.Null(createdMatch.EndTime);

            // Verify table is updated with the new match ID
            FoosballTableModel dbTable = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables").First();
            Assert.Equal(createdMatch.Id, dbTable.ActiveMatchId);

            // Verify that only one new team is created (total of 2 teams in the database)
            IEnumerable<TeamDbModel> teams = _dbHelper.ReadData<TeamDbModel>("SELECT * FROM teams");
            Assert.Equal(2, teams.Count());

            // Verify existing and new teams' user assignments
            TeamDbModel dbRedTeam = teams.FirstOrDefault(t => t.Id == createdMatch.RedTeamId);
            TeamDbModel dbBlueTeam = teams.FirstOrDefault(t => t.Id == createdMatch.BlueTeamId);

            Assert.NotNull(dbRedTeam);
            Assert.NotNull(dbBlueTeam);

            // Assert that the existing team is reused as the red team
            Assert.True(dbRedTeam.Id == teams.First().Id);
            Assert.True(teams.First().User1Id == 1 && teams.First().User2Id == null);

            // Assert that the new blue team has the correct user assignments
            Assert.True(dbBlueTeam.Id == teams.Last().Id);
            Assert.True(teams.Last().User1Id == 2 && teams.Last().User2Id == 3);
        }
    }
}
