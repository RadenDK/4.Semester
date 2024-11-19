using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.BusinessLogic.Interfaces;
using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.DatabaseAccess.Interfaces;
using FoosballProLeague.Api.Models.DbModels;
using FoosballProLeague.Api.Models.FoosballModels;
using FoosballProLeague.Api.Models.RequestModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Moq;


namespace FoosballProLeague.Api.Tests
{
    // SUT = System Under Test

    [Collection("Non-Parallel Database Collection")]
    public class TableLoginTests : DatabaseTestBase
    {

        // Test: Login when the table is empty with one player
        [Fact]
        public void Login_WhenTableIsEmpty_ShouldRegisterOnePlayer()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id) VALUES (1)");
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");

            int mockTableId = 1;
            TableLoginRequest mockTableLoginRequest = new TableLoginRequest { UserId = 1, TableId = mockTableId, Side = "red" };

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);
            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, teamDatabaseAccessor, mockHubContext.Object, userLogic);
            
            MatchController SUT = new MatchController(matchLogic);

            // Act
            IActionResult result = SUT.LoginOnTable(mockTableLoginRequest);

            // Assert
            IEnumerable<MatchModel> football_matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> football_table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables where id = " + mockTableId);
            IEnumerable<TeamModel> teams = _dbHelper.ReadData<TeamModel>("SELECT * FROM teams");

            Assert.IsType<OkObjectResult>(result); // Expect the login to succeed

            // Both Asserts should be empty because a match and teams are only created when a goal is registered
            Assert.Empty(football_matches);
            Assert.Empty(teams);

            Assert.True(football_table.First().ActiveMatchId == null);
        }

        // Test: Multiple logins on a table that was initially empty
        [Fact]
        public void Login_MultiplePlayersOnEmptyTable_ShouldRegisterMultiplePlayers()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id) VALUES (1), (2), (3), (4)");
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");

            int mockTableId = 1;
            TableLoginRequest mockTableLoginRequest1 = new TableLoginRequest { UserId = 1, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequest2 = new TableLoginRequest { UserId = 2, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequest3 = new TableLoginRequest { UserId = 3, TableId = mockTableId, Side = "blue" };
            TableLoginRequest mockTableLoginRequest4 = new TableLoginRequest { UserId = 4, TableId = mockTableId, Side = "blue" };

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);
            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, teamDatabaseAccessor, mockHubContext.Object, userLogic);

            MatchController SUT = new MatchController(matchLogic);

            // Act
            IActionResult result1 = SUT.LoginOnTable(mockTableLoginRequest1);
            IActionResult result2 = SUT.LoginOnTable(mockTableLoginRequest2);
            IActionResult result3 = SUT.LoginOnTable(mockTableLoginRequest3);
            IActionResult result4 = SUT.LoginOnTable(mockTableLoginRequest4);

            // Assert
            IEnumerable<MatchModel> football_matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> football_table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables where id = " + mockTableId);
            IEnumerable<TeamModel> teams = _dbHelper.ReadData<TeamModel>("SELECT * FROM teams");

            Assert.IsType<OkObjectResult>(result1);
            Assert.IsType<OkObjectResult>(result2);
            Assert.IsType<OkObjectResult>(result3);
            Assert.IsType<OkObjectResult>(result4);

            // Both Asserts should be empty because a match and teams are only created when a goal is registered
            Assert.Empty(football_matches);
            Assert.Empty(teams);

            Assert.True(football_table.First().ActiveMatchId == null);
        }

        // Test: Login on full team (a team with two players)
        [Fact]
        public void Login_WhenTeamIsFull_ShouldNotAllowAdditionalPlayers()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id) VALUES (1), (2), (3)");
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");

            int mockTableId = 1;
            TableLoginRequest mockTableLoginRequest1 = new TableLoginRequest { UserId = 1, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequest2 = new TableLoginRequest { UserId = 2, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequest3 = new TableLoginRequest { UserId = 3, TableId = mockTableId, Side = "red" };

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);
            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, teamDatabaseAccessor, mockHubContext.Object, userLogic);
            
            MatchController SUT = new MatchController(matchLogic);

            // Act
            SUT.LoginOnTable(mockTableLoginRequest1); // First player logs in on the red side
            SUT.LoginOnTable(mockTableLoginRequest2); // Second player logs in on the red side
            IActionResult result = SUT.LoginOnTable(mockTableLoginRequest3); // Third player tries to log in on the red side

            // Assert

            IEnumerable<MatchModel> football_matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> football_table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables where id = " + mockTableId);
            IEnumerable<TeamModel> teams = _dbHelper.ReadData<TeamModel>("SELECT * FROM teams");

            Assert.IsType<BadRequestObjectResult>(result); // Expect the third login to return a BadRequestObjectResult

            // Both Asserts should be empty because a match and teams are only created when a goal is registered
            Assert.Empty(football_matches);
            Assert.Empty(teams);

            Assert.True(football_table.First().ActiveMatchId == null);

        }

        // Test: Login on full table (both teams have two players)
        [Fact]
        public void Login_WhenTableIsFull_ShouldNotAllowAdditionalPlayers()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id) VALUES (1), (2), (3), (4), (5), (6)");
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");

            int mockTableId = 1;
            TableLoginRequest mockTableLoginRequest1 = new TableLoginRequest { UserId = 1, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequest2 = new TableLoginRequest { UserId = 2, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequest3 = new TableLoginRequest { UserId = 3, TableId = mockTableId, Side = "blue" };
            TableLoginRequest mockTableLoginRequest4 = new TableLoginRequest { UserId = 4, TableId = mockTableId, Side = "blue" };
            TableLoginRequest mockTableLoginRequest5 = new TableLoginRequest { UserId = 5, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequest6 = new TableLoginRequest { UserId = 6, TableId = mockTableId, Side = "blue" };


            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);
            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, teamDatabaseAccessor, mockHubContext.Object, userLogic);
            
            MatchController SUT = new MatchController(matchLogic);

            // Act
            SUT.LoginOnTable(mockTableLoginRequest1); // First red player
            SUT.LoginOnTable(mockTableLoginRequest2); // Second red player
            SUT.LoginOnTable(mockTableLoginRequest3); // First blue player
            SUT.LoginOnTable(mockTableLoginRequest4); // Second blue player
            IActionResult result1 = SUT.LoginOnTable(mockTableLoginRequest5); // Third red player attempts to log in
            IActionResult result2 = SUT.LoginOnTable(mockTableLoginRequest6); // Third blue player attempts to log in


            // Assert

            IEnumerable<MatchModel> football_matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> football_table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables where id = " + mockTableId);
            IEnumerable<TeamModel> teams = _dbHelper.ReadData<TeamModel>("SELECT * FROM teams");

            Assert.IsType<BadRequestObjectResult>(result1); // Expect the third red login to return a BadRequestObjectResult
            Assert.IsType<BadRequestObjectResult>(result2); // Expect the third blue login to return a BadRequestObjectResult

            // Both Asserts should be empty because a match and teams are only created when a goal is registered
            Assert.Empty(football_matches);
            Assert.Empty(teams);

            Assert.True(football_table.First().ActiveMatchId == null);

        }

        // Test: Login allowed when match is active but there is room on the team
        [Fact]
        public void Login_WhenMatchIsActiveAndTeamHasRoom_ShouldAllowLogin()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id, first_name, last_name) VALUES (1, 'firstname1', 'lastname1'), (2, 'firstname2', 'lastname2'), (3, 'firstname3', 'lastname3')");
            _dbHelper.InsertData("INSERT INTO teams (player1_id) VALUES (1), (2)");
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");
            _dbHelper.InsertData("INSERT INTO foosball_matches (id, table_id, red_team_id, blue_team_id) VALUES (1, 1, 1, 2)"); // Active match
            _dbHelper.UpdateData("UPDATE foosball_tables SET active_match_id = 1 WHERE id = 1");

            int mockTableId = 1;
            TableLoginRequest mockTableLoginRequest = new TableLoginRequest { UserId = 3, TableId = mockTableId, Side = "red" }; // Attempt to join the red side

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);
            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, teamDatabaseAccessor, mockHubContext.Object, userLogic);
            
            MatchController SUT = new MatchController(matchLogic);

            // Act
            IActionResult result = SUT.LoginOnTable(mockTableLoginRequest); // Player tries to log in during an active match

            // Assert
            Assert.IsType<OkObjectResult>(result); // Expect the login to succeed since there is room on the red team

            IEnumerable<MatchDbModel> dbFossballMatches = _dbHelper.ReadData<MatchDbModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> foosballTable = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables where id = " + mockTableId);
            IEnumerable<TeamDbModel> dbTeams = _dbHelper.ReadData<TeamDbModel>("SELECT * FROM teams");

            // There should be an active match
            Assert.True(dbFossballMatches.Count() == 1);
            Assert.True(dbFossballMatches.First().RedTeamId == 3);

            MatchDbModel dbMatch = dbFossballMatches.First();
            Assert.True(dbMatch.Id == 1);

            Assert.True(foosballTable.First().ActiveMatchId == dbMatch.Id);

            // Teams should not be empty since they are already registered
            Assert.True(dbTeams.Count() == 3);

            TeamDbModel firstTeam = dbTeams.Where(t => t.Id == 1).FirstOrDefault();
            TeamDbModel secondTeam = dbTeams.Where(t => t.Id == 2).FirstOrDefault();
            TeamDbModel thirdTeam = dbTeams.Where(t => t.Id == 3).FirstOrDefault();

            // There should be added an user to red team
            Assert.True(firstTeam.Player1Id == 1 && firstTeam.Player2Id == null);
            Assert.True(secondTeam.Player1Id == 2 && secondTeam.Player2Id == null);
            Assert.True(thirdTeam.Player1Id == 1 && thirdTeam.Player2Id == 3);
        }

        // Test: Login on table with an active match and full team
        [Fact]
        public void Login_WhenMatchIsActiveAndTeamIsFull_ShouldNotAllowNewLogins()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id) VALUES (1), (2), (3), (4), (5)");
            _dbHelper.InsertData("INSERT INTO teams (id, player1_id, player2_id) VALUES (1, 1, 2), (2, 3, 4)"); // Red and Blue teams are full
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");
            _dbHelper.InsertData("INSERT INTO foosball_matches (id, table_id, red_team_id, blue_team_id) VALUES (1, 1, 1, 2)"); // Active match
            _dbHelper.UpdateData("UPDATE foosball_tables SET active_match_id = 1 WHERE id = 1");

            int mockTableId = 1;
            TableLoginRequest mockTableLoginRequest = new TableLoginRequest { UserId = 5, TableId = mockTableId, Side = "red" }; // Attempt to join the red side

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);
            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, teamDatabaseAccessor, mockHubContext.Object, userLogic);
            
            MatchController SUT = new MatchController(matchLogic);

            // Act
            IActionResult result = SUT.LoginOnTable(mockTableLoginRequest); // Player tries to log in during an active match

            // Assert

            IEnumerable<MatchModel> football_matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> football_table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables where id = " + mockTableId);
            IEnumerable<TeamModel> teams = _dbHelper.ReadData<TeamModel>("SELECT * FROM teams");

            Assert.IsType<BadRequestObjectResult>(result); // Expect the login to fail since the match is active and the red team is full

            // Assert that there is only one match, which is the one inserted in the database
            Assert.Single(football_matches);
            Assert.Equal(1, football_matches.First().Id);

            // Assert that there are only the teams that were inserted in the database
            Assert.Equal(2, teams.Count());

            Assert.True(football_table.First().ActiveMatchId == 1);
        }

        // Test: Existing team should be reused if players are the same
        [Fact]
        public void RegisterGoal_WhenPreviousTeamExists_ShouldReuseExistingTeamId()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id) VALUES (1), (2), (3)");
            _dbHelper.InsertData("INSERT INTO teams (player1_id) VALUES (1), (2)");
            _dbHelper.InsertData("INSERT INTO teams (player1_id, player2_id) VALUES (1, 3)");
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");
            _dbHelper.InsertData("INSERT INTO foosball_matches (id, table_id, red_team_id, blue_team_id) VALUES (1, 1, 1, 2)"); // Active match
            _dbHelper.UpdateData("UPDATE foosball_tables SET active_match_id = 1 WHERE id = 1");

            int mockTableId = 1;
            TableLoginRequest mockTableLoginRequest = new TableLoginRequest { UserId = 3, TableId = mockTableId, Side = "red" }; // Attempt to join the red side

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);
            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, teamDatabaseAccessor, mockHubContext.Object, userLogic);
            
            MatchController SUT = new MatchController(matchLogic);

            // Act
            IActionResult result = SUT.LoginOnTable(mockTableLoginRequest); // Player tries to log in during an active match

            // Assert
            Assert.IsType<OkObjectResult>(result); // Expect the login to succeed since there is room on the red team

            IEnumerable<MatchModel> football_matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> football_table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables where id = " + mockTableId);
            IEnumerable<TeamModel> teams = _dbHelper.ReadData<TeamModel>("SELECT * FROM teams");

            // Assert that there is still only one match, which is the one inserted in the database
            Assert.Single(football_matches);
            Assert.Equal(1, football_matches.First().Id);

            // Assert that the match is still active for the table
            Assert.Equal(1, football_table.First().ActiveMatchId);

            // Assert that no other teams have been created other than the teams inserted in the database
            Assert.Equal(3, teams.Count());
        }
    }
}
