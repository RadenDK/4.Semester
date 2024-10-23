using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.Models.FoosballModels;
using FoosballProLeague.Api.Models.RequestModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoosballProLeague.Api.Tests.ControllerTests.IntergrationTests.MatchControllerTests
{
    // SUT = System Under Test

    [Collection("Non-Parallel Database Collection")]
    public class ScoringTests : DatabaseTestBase
    {
        // Test: Goal scored on non-active table but with valid pending teams
        [Fact]
        public void GoalScored_OnNonActiveTableWithValidPendingTeams_ShouldCreateMatchAndRegisterGoal()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id) VALUES (1), (2)");
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");

            int mockTableId = 1;
            TableLoginRequest mockTableLoginRequestPlayer1 = new TableLoginRequest { PlayerId = 1, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequestPlayer2 = new TableLoginRequest { PlayerId = 2, TableId = mockTableId, Side = "blue" };

            RegisterGoalRequest mockRegisterGoalRequest = new RegisterGoalRequest { TableId = mockTableId, Side = "red" };

            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor);
            MatchController SUT = new MatchController(matchLogic);

            // Act
            SUT.LoginOnTable(mockTableLoginRequestPlayer1);
            SUT.LoginOnTable(mockTableLoginRequestPlayer2);
            IActionResult result = SUT.RegisterGoal(mockRegisterGoalRequest);

            // Assert
            IEnumerable<MatchModel> matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables");
            IEnumerable<TeamModel> teams = _dbHelper.ReadData<TeamModel>("SELECT * FROM teams");

            Assert.IsType<OkResult>(result); // Check if the result is an OkResult
            Assert.True(matches.First() != null); // Check if the match was created
            Assert.True(matches.First().RedTeamId == 1 && matches.First().BlueTeamId == 2); // Check if the correct teams were assigned to the match
            Assert.True(matches.First().TeamRedScore == 1 && matches.First().TeamBlueScore == 0); // Check if the goal was registered correctly
            Assert.True(matches.First().StartTime.AddSeconds(5) >= DateTime.Now); // Check if the match start time is correct
            Assert.True(matches.First().EndTime == null); // Check if the match has not ended
            Assert.True(table.First().ActiveMatchId == matches.First().Id); // Check if the table has the correct active match
            Assert.True(teams.Count() == 2); // Check if the correct teams were created
            Assert.True(teams.First().Player1Id == 1 && teams.First().Player2Id == null); // Check if the correct players were assigned to the team
            Assert.True(teams.Last().Player1Id == 2 && teams.Last().Player2Id == null); // Check if the correct players were assigned to the team
        }


        // Test: Goal scored on table with ongoing match and existing teams
        [Fact]
        public void GoalScored_WithOngoingMatchAndExistingTeams_ShouldRegisterGoalWithoutCreatingNewTeamsOrMatch()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id) VALUES (1), (2)");
            _dbHelper.InsertData("INSERT INTO teams (id, player1_id) VALUES (1, 1), (2, 2)");
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");
            _dbHelper.InsertData("INSERT INTO foosball_matches (id, table_id, red_team_id, blue_team_id, team_red_score, team_blue_score) VALUES (1, 1, 1, 2, 4, 0)"); // Active match where red has scored 4 goals and blue 0
            _dbHelper.UpdateData("UPDATE foosball_tables SET active_match_id = 1 WHERE id = 1");

            int mockTableId = 1;

            RegisterGoalRequest mockRegisterGoalRequest = new RegisterGoalRequest { TableId = mockTableId, Side = "red" };

            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor);
            MatchController SUT = new MatchController(matchLogic);

            // Act
            IActionResult result = SUT.RegisterGoal(mockRegisterGoalRequest);

            // Assert
            IEnumerable<MatchModel> matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables");
            IEnumerable<TeamModel> teams = _dbHelper.ReadData<TeamModel>("SELECT * FROM teams");

            Assert.IsType<OkResult>(result); // Check if the result is an OkResult
            Assert.True(matches.Count() == 1); // Ensure no new match was created
            Assert.True(matches.First().RedTeamId == 1 && matches.First().BlueTeamId == 2); // Check if the correct teams are still in the match
            Assert.True(matches.First().TeamRedScore == 5 && matches.First().TeamBlueScore == 0); // Ensure the goal was registered correctly
            Assert.True(matches.First().StartTime.AddSeconds(5) >= DateTime.Now); // Check if the match start time is correct
            Assert.True(matches.First().EndTime == null); // Ensure the match is still ongoing
            Assert.True(table.First().ActiveMatchId == matches.First().Id); // Check if the table still has the correct active match
            Assert.True(teams.Count() == 2); // Ensure no new teams were created
            Assert.True(teams.First().Player1Id == 1 && teams.First().Player2Id == null); // Check if the correct players were assigned to the red team
            Assert.True(teams.Last().Player1Id == 2 && teams.Last().Player2Id == null); // Check if the correct players were assigned to the blue team
        }


        // Test: Goal scored with uneven teams (1 player on one side, 2 on the other) should still be registered
        [Fact]
        public void GoalScored_WithUnbalancedTeams_ShouldRegisterGoalCorrectly()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id) VALUES (1), (2), (3)");
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");

            int mockTableId = 1;
            TableLoginRequest mockTableLoginRequestPlayer1 = new TableLoginRequest { PlayerId = 1, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequestPlayer2 = new TableLoginRequest { PlayerId = 2, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequestPlayer3 = new TableLoginRequest { PlayerId = 3, TableId = mockTableId, Side = "blue" };

            RegisterGoalRequest mockRegisterGoalRequest = new RegisterGoalRequest { TableId = mockTableId, Side = "red" };

            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor);
            MatchController SUT = new MatchController(matchLogic);

            // Act
            SUT.LoginOnTable(mockTableLoginRequestPlayer1);
            SUT.LoginOnTable(mockTableLoginRequestPlayer2);
            SUT.LoginOnTable(mockTableLoginRequestPlayer3);

            IActionResult result = SUT.RegisterGoal(mockRegisterGoalRequest);

            // Assert
            IEnumerable<MatchModel> matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables");
            IEnumerable<TeamModel> teams = _dbHelper.ReadData<TeamModel>("SELECT * FROM teams");

            Assert.IsType<OkResult>(result); // Check if the result is an OkResult
            Assert.True(matches.Count() == 1); // Ensure no new match was created
            Assert.True(matches.First().RedTeamId == 1 && matches.First().BlueTeamId == 2); // Check if the correct teams were assigned to the match
            Assert.True(matches.First().TeamRedScore == 1 && matches.First().TeamBlueScore == 0); // Ensure the goal was registered correctly
            Assert.True(matches.First().StartTime.AddSeconds(5) >= DateTime.Now); // Check if the match start time is correct
            Assert.True(matches.First().EndTime == null); // Ensure the match has not ended
            Assert.True(table.First().ActiveMatchId == matches.First().Id); // Check if the table still has the correct active match
            Assert.True(teams.Count() == 2); // Ensure no new teams were created
            Assert.True(teams.First().Player1Id == 1 && teams.First().Player2Id == 2); // Check if the correct players were assigned to the red team
            Assert.True(teams.Last().Player1Id == 3 && teams.Last().Player2Id == null); // Check if the correct player was assigned to the blue team
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

            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor);
            MatchController SUT = new MatchController(matchLogic);

            // Act
            IActionResult result = SUT.RegisterGoal(mockRegisterGoalRequestRedSide);

            // Assert
            IEnumerable<MatchModel> matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables");

            Assert.True(matches.First() != null); // Check if the match was created
            Assert.True(matches.First().RedTeamId == 1 && matches.First().BlueTeamId == 2); // Check if the correct teams were assigned to the match
            Assert.True(matches.First().TeamRedScore == 10 && matches.First().TeamBlueScore == 0); // Check if the goal was registered correctly
            Assert.True(matches.First().StartTime.AddSeconds(5) >= DateTime.Now); // Check if the match start time is correct
            Assert.True(matches.First().EndTime.Value.AddSeconds(5) >= DateTime.Now); // Check if the match end time is set correctly
            Assert.True(table.First().ActiveMatchId == null); // Ensure the table's active match ID is set to null since the match is complete
        }

        // Test: Red team scores 10 goals, new players log in, and a new match is started after a goal is scored
        [Fact]
        public void GoalScored_AfterRedTeamScores10Goals_ShouldCompleteOldMatchAndStartNewMatchWithNewPlayers()
        {
            // Arrange

            // If id's not included thats because the code will fail due to postgresql id sequence being manually overwritted by specifying the id in these inserts
            _dbHelper.InsertData("INSERT INTO users (id) VALUES (1), (2), (3), (4)");
            _dbHelper.InsertData("INSERT INTO teams (player1_id) VALUES (1), (2)"); // Existing teams for the first match. 
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");
            _dbHelper.InsertData("INSERT INTO foosball_matches (table_id, red_team_id, blue_team_id, team_red_score, team_blue_score) VALUES (1, 1, 2, 9, 0)"); // Active match with red team at 9 goals
            _dbHelper.UpdateData("UPDATE foosball_tables SET active_match_id = 1 WHERE id = 1");

            int mockTableId = 1;

            RegisterGoalRequest mockRegisterGoalRequestRedSide = new RegisterGoalRequest { TableId = mockTableId, Side = "red" };

            TableLoginRequest mockTableLoginRequestPlayer3 = new TableLoginRequest { PlayerId = 3, TableId = mockTableId, Side = "red" }; // New players for the new match
            TableLoginRequest mockTableLoginRequestPlayer4 = new TableLoginRequest { PlayerId = 4, TableId = mockTableId, Side = "blue" };

            RegisterGoalRequest mockRegisterGoalRequestNewMatch = new RegisterGoalRequest { TableId = mockTableId, Side = "blue" };

            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor);
            MatchController SUT = new MatchController(matchLogic);

            // Act
            // Red team scores the 10th goal, completing the match
            IActionResult resultRedGoal = SUT.RegisterGoal(mockRegisterGoalRequestRedSide);

            // New players log in for the next match
            SUT.LoginOnTable(mockTableLoginRequestPlayer3);
            SUT.LoginOnTable(mockTableLoginRequestPlayer4);

            // A new goal is scored for the new match
            IActionResult resultNewGoal = SUT.RegisterGoal(mockRegisterGoalRequestNewMatch);

            // Assert
            IEnumerable<MatchModel> matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables");

            // Assertions for the old match
            var oldMatch = matches.First(); // The first match
            Assert.True(oldMatch.RedTeamId == 1 && oldMatch.BlueTeamId == 2); // Check the teams for the old match
            Assert.True(oldMatch.TeamRedScore == 10 && oldMatch.TeamBlueScore == 0); // Check that the red team reached 10 goals
            Assert.True(oldMatch.EndTime.HasValue); // Ensure the old match is marked as finished

            // Assertions for the new match
            var newMatch = matches.Last(); // The new match
            Assert.True(newMatch.RedTeamId != oldMatch.RedTeamId); // Ensure it's a new match with different teams
            Assert.True(newMatch.TeamRedScore == 0 && newMatch.TeamBlueScore == 1); // Check that the new match has 1 goal for blue team
            Assert.True(newMatch.StartTime.AddSeconds(5) >= DateTime.Now); // Check if the new match start time is correct
            Assert.True(newMatch.EndTime == null); // The new match should still be ongoing

            // Assert that the table is now associated with the new match
            Assert.True(table.First().ActiveMatchId == newMatch.Id); // Ensure the table is linked to the new active match
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

            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor);
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
            IEnumerable<MatchModel> matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables");

            // Ensure only one match was created
            Assert.True(matches.Count() == 1); // Ensure no new match was created
            var match = matches.First();

            // Check the match teams and score
            Assert.True(match.RedTeamId == 1 && match.BlueTeamId == 2); // Check if the correct teams were assigned to the match
            Assert.True(match.TeamRedScore == 3 && match.TeamBlueScore == 4); // Ensure the goals were registered correctly

            // Ensure the match is still ongoing
            Assert.True(match.EndTime == null); // Check that the match is not finished

            // Check if the table still has the correct active match
            Assert.True(table.First().ActiveMatchId == match.Id);
        }

        // Test: Goal registered with no players logged in should return BadRequest and not create match or teams
        [Fact]
        public void GoalScored_WithNoPlayersLoggedIn_ShouldReturnBadRequestAndNotCreateMatchOrTeams()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");

            int mockTableId = 1;
            RegisterGoalRequest mockRegisterGoalRequestRedSide = new RegisterGoalRequest { TableId = mockTableId, Side = "red" };

            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor);
            MatchController SUT = new MatchController(matchLogic);

            // Act
            IActionResult result = SUT.RegisterGoal(mockRegisterGoalRequestRedSide);

            // Assert
            // Ensure the result is a BadRequest
            Assert.IsType<BadRequestResult>(result);

            // Ensure no match was created
            IEnumerable<MatchModel> matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            Assert.True(matches.Count() == 0); // No match should be created

            // Ensure no teams were created
            IEnumerable<TeamModel> teams = _dbHelper.ReadData<TeamModel>("SELECT * FROM teams");
            Assert.True(teams.Count() == 0); // No team should be created

            // Ensure the table's active match ID remains null
            IEnumerable<FoosballTableModel> table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables");
            Assert.True(table.First().ActiveMatchId == null); // No active match should be set
        }


    }
}
