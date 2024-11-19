using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.BusinessLogic.Interfaces;
using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.DatabaseAccess.Interfaces;
using FoosballProLeague.Api.Models.DbModels;
using FoosballProLeague.Api.Models.FoosballModels;
using FoosballProLeague.Api.Models.RequestModels;
using Microsoft.AspNetCore.Mvc;

namespace FoosballProLeague.Api.Tests.ControllerTests.IntergrationTests.MatchControllerTests
{
    // SUT = System Under Test

    [Collection("Non-Parallel Database Collection")]
    public class ScoringTests : DatabaseTestBase
    {
        // Test: Goal registered on table with no active match should return BadRequest
        [Fact]
        public void GoalScored_WithNoPlayersLoggedIn_ShouldReturnBadRequest()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");

            int mockTableId = 1;
            RegisterGoalRequest mockRegisterGoalRequestRedSide = new RegisterGoalRequest { TableId = mockTableId, Side = "red" };

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);
            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, teamDatabaseAccessor, mockHubContext.Object, userLogic);

            MatchController SUT = new MatchController(matchLogic);

            // Act
            IActionResult result = SUT.RegisterGoal(mockRegisterGoalRequestRedSide);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result); // Ensure the result is a BadRequest

            // Ensure the table's active match ID remains null
            IEnumerable<FoosballTableModel> table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables");
            Assert.True(table.First().ActiveMatchId == null); // No active match should be set

            // Ensure no match logs were created
            IEnumerable<MatchLogModel> matchLogs = _dbHelper.ReadData<MatchLogModel>("SELECT * FROM match_logs");
            Assert.Empty(matchLogs); // No match logs should be created
        }

        // Test: Goal scored on an active table with a matched started should register the goal
        [Fact]
        public void GoalScored_OnActiveTable_ShouldRegisterGoal()
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

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, teamDatabaseAccessor, mockHubContext.Object, userLogic);

            MatchController SUT = new MatchController(matchLogic);

            // Act
            IActionResult result = SUT.RegisterGoal(mockRegisterGoalRequest);

            // Assert
            IEnumerable<MatchModel> matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables");
            IEnumerable<MatchLogModel> matchLogs = _dbHelper.ReadData<MatchLogModel>("SELECT * FROM match_logs");

            Assert.IsType<OkObjectResult>(result); // Check if the result is an OkResult
            Assert.True(matches.First().TeamRedScore == 5 && matches.First().TeamBlueScore == 0); // Check if the goal was registered correctly
            Assert.True(matches.First().EndTime == null); // Check if the match has not ended
            Assert.True(table.First().ActiveMatchId == matches.First().Id); // Check if the table has the correct active match
            Assert.True(matchLogs.Any(), "A match log should be created when a goal is registered"); // Check if a match log was created
        }


        // Test: Goal scored on table with ongoing match
        [Fact]
        public void GoalScored_WithOngoingMatch_ShouldRegisterGoalWithoutEndingMatch()
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

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, teamDatabaseAccessor, mockHubContext.Object, userLogic);
            
            MatchController SUT = new MatchController(matchLogic);

            // Act
            IActionResult result = SUT.RegisterGoal(mockRegisterGoalRequest);

            // Assert
            IEnumerable<MatchModel> matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables");
            IEnumerable<MatchLogModel> matchLogs = _dbHelper.ReadData<MatchLogModel>("SELECT * FROM match_logs");

            Assert.IsType<OkObjectResult>(result); // Check if the result is an OkResult
            Assert.True(matches.First().TeamRedScore == 5 && matches.First().TeamBlueScore == 0); // Ensure the goal was registered correctly
            Assert.True(matches.First().EndTime == null); // Ensure the match is still ongoing
            Assert.True(table.First().ActiveMatchId == matches.First().Id); // Check if the table still has the correct active match
            Assert.True(matchLogs.Count() == 1, "There should be exactly one match log created when a goal is registered"); // Check if exactly one match log was created
        }


        // Test: Goal scored with uneven teams (1 player on one side, 2 on the other) should still be registered
        [Fact]
        public void GoalScored_WithUnbalancedTeams_ShouldRegisterGoalCorrectly()
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

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, teamDatabaseAccessor, mockHubContext.Object, userLogic);

            MatchController SUT = new MatchController(matchLogic);

            // Act
            IActionResult result = SUT.RegisterGoal(mockRegisterGoalRequest);

            // Assert
            IEnumerable<MatchModel> matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables");
            IEnumerable<TeamModel> teams = _dbHelper.ReadData<TeamModel>("SELECT * FROM teams");
            IEnumerable<MatchLogModel> matchLogs = _dbHelper.ReadData<MatchLogModel>("SELECT * FROM match_logs");

            Assert.IsType<OkObjectResult>(result); // Check if the result is an OkResult
            Assert.True(matches.First().TeamRedScore == 1 && matches.First().TeamBlueScore == 0); // Ensure the goal was registered correctly
            Assert.True(matches.First().EndTime == null); // Ensure the match has not ended
            Assert.True(table.First().ActiveMatchId == matches.First().Id); // Check if the table still has the correct active match
            Assert.True(matchLogs.Count() == 1, "There should be exactly one match log created when a goal is registered"); // Check if exactly one match log was created
        }

        // Test: Running goals will be registered on the same match
        [Fact]
        public void GoalScored_RunningGoalsWillBeRegisteredOnSameMatch()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id) VALUES (1), (2)");
            _dbHelper.InsertData("INSERT INTO teams (id, player1_id) VALUES (1, 1), (2, 2)");
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");
            _dbHelper.InsertData("INSERT INTO foosball_matches (id, table_id, red_team_id, blue_team_id, team_red_score, team_blue_score) VALUES (1, 1, 1, 2, 0, 0)"); // Active match where red has scored 0 goals and blue 0
            _dbHelper.UpdateData("UPDATE foosball_tables SET active_match_id = 1 WHERE id = 1");

            int mockTableId = 1;

            RegisterGoalRequest mockRegisterGoalRequestRedSide = new RegisterGoalRequest { TableId = mockTableId, Side = "red" };
            RegisterGoalRequest mockRegisterGoalRequestBlueSide = new RegisterGoalRequest { TableId = mockTableId, Side = "blue" };
IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);
            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, teamDatabaseAccessor, mockHubContext.Object, userLogic);

            MatchController SUT = new MatchController(matchLogic);

            // Act
            SUT.RegisterGoal(mockRegisterGoalRequestRedSide);
            SUT.RegisterGoal(mockRegisterGoalRequestRedSide);
            SUT.RegisterGoal(mockRegisterGoalRequestBlueSide);
            SUT.RegisterGoal(mockRegisterGoalRequestRedSide);
            SUT.RegisterGoal(mockRegisterGoalRequestBlueSide);
            SUT.RegisterGoal(mockRegisterGoalRequestBlueSide);
            SUT.RegisterGoal(mockRegisterGoalRequestBlueSide);

            // Assert
            IEnumerable<MatchDbModel> dbMatches = _dbHelper.ReadData<MatchDbModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables");
            IEnumerable<MatchLogModel> matchLogs = _dbHelper.ReadData<MatchLogModel>("SELECT * FROM match_logs");

            // Ensure only one match was created
            Assert.True(dbMatches.Count() == 1); // Ensure no new match was created
            MatchDbModel dbMatch = dbMatches.First();

            // Check the match teams and score
            Assert.True(dbMatch.RedTeamId == 1 && dbMatch.BlueTeamId == 2); // Check if the correct teams were assigned to the match
            Assert.True(dbMatch.TeamRedScore == 3 && dbMatch.TeamBlueScore == 4); // Ensure the goals were registered correctly

            // Ensure the match is still ongoing
            Assert.True(dbMatch.EndTime == null); // Check that the match is not finished

            // Check if the table still has the correct active match
            Assert.True(table.First().ActiveMatchId == dbMatch.Id);

            // Ensure a match log was created for each goal registered
            Assert.Equal(7, matchLogs.Count()); // There should be exactly 7 match logs created
        }


        // Test: Goal scored when side reaches 10 goals should complete the match and set active match to null
        [Fact]
        public void GoalScored_WhenSideReaches10Goals_ShouldCompleteMatchAndSetActiveMatchToNull()
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

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, teamDatabaseAccessor, mockHubContext.Object, userLogic);

            MatchController SUT = new MatchController(matchLogic);

            // Act
            IActionResult result = SUT.RegisterGoal(mockRegisterGoalRequestRedSide);

            // Assert
            IEnumerable<MatchModel> matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables");
            IEnumerable<MatchLogModel> matchLogs = _dbHelper.ReadData<MatchLogModel>("SELECT * FROM match_logs");

            Assert.True(matches.First() != null);
            Assert.True(matches.First().TeamRedScore == 10 && matches.First().TeamBlueScore == 0);
            Assert.True(matches.First().EndTime.Value.AddSeconds(5) >= DateTime.Now);
            Assert.True(table.First().ActiveMatchId == null);

            Assert.True(matchLogs.Count() == 1, "There should be exactly one match log created when the match is completed");
        }

        // Test: Red team scores 10 goals, completing the match, and any new goal registration should fail
        [Fact]
        public void GoalScored_AfterRedTeamScores10Goals_ShouldCompleteOldMatchNewRegistergoalRequestShouldFail()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id) VALUES (1), (2)");
            _dbHelper.InsertData("INSERT INTO teams (player1_id) VALUES (1), (2)");
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");
            _dbHelper.InsertData("INSERT INTO foosball_matches (table_id, red_team_id, blue_team_id, team_red_score, team_blue_score) VALUES (1, 1, 2, 9, 0)");
            _dbHelper.UpdateData("UPDATE foosball_tables SET active_match_id = 1 WHERE id = 1");

            int mockTableId = 1;

            RegisterGoalRequest mockRegisterGoalRequestRedSide = new RegisterGoalRequest { TableId = mockTableId, Side = "red" };
            RegisterGoalRequest mockRegisterGoalRequestAfterMatchEnd = new RegisterGoalRequest { TableId = mockTableId, Side = "blue" };

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);
            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, teamDatabaseAccessor, mockHubContext.Object, userLogic);

            MatchController SUT = new MatchController(matchLogic);

            // Act
            IActionResult result = SUT.RegisterGoal(mockRegisterGoalRequestRedSide);
            IActionResult resultAfterMatchEnd = SUT.RegisterGoal(mockRegisterGoalRequestAfterMatchEnd);

            // Assert
            Assert.IsType<OkObjectResult>(result);

            IEnumerable<MatchModel> matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables");
            IEnumerable<MatchLogModel> matchLogs = _dbHelper.ReadData<MatchLogModel>("SELECT * FROM match_logs");

            Assert.Single(matches);
            Assert.Equal(10, matches.First().TeamRedScore);
            Assert.Equal(0, matches.First().TeamBlueScore);
            Assert.NotNull(matches.First().EndTime);
            Assert.True(matches.First().EndTime.Value.AddSeconds(5) >= DateTime.Now);
            Assert.Null(table.First().ActiveMatchId);
            Assert.Single(matchLogs);
            Assert.IsType<BadRequestObjectResult>(resultAfterMatchEnd);
        }
    }
}
