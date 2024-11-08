using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.Models.FoosballModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Moq;


namespace FoosballProLeague.Api.Tests.ControllerTests.IntergrationTests.MatchControllerTests
{
    [Collection("Non-Parallel Database Collection")]
    public class InterruptMatchTests : DatabaseTestBase
    {
        [Fact]
        public void InterruptMatch_WithActiveMatch_ShouldReturnSuccess()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id) VALUES (1), (2)");
            _dbHelper.InsertData("INSERT INTO teams (player1_id) VALUES (1), (2)"); // Existing teams for the match
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");
            _dbHelper.InsertData("INSERT INTO foosball_matches (table_id, red_team_id, blue_team_id, team_red_score, team_blue_score) VALUES (1, 1, 2, 5, 3)"); // Active match
            _dbHelper.UpdateData("UPDATE foosball_tables SET active_match_id = 1 WHERE id = 1");

            int mockTableId = 1;

            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()));
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, userLogic);
            MatchController SUT = new MatchController(matchLogic);

            // Act
            IActionResult result = SUT.InterruptMatch(mockTableId);

            // Assert
            IEnumerable<MatchModel> matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables");

            MatchModel interruptedMatch = matches.First(); // The match that was interrupted
            Assert.True(interruptedMatch.EndTime.HasValue); // Ensure the match end time is set
            Assert.True(table.First().ActiveMatchId == null); // Ensure the table's active match ID is set to null since the match is interrupted
        }

        [Fact]
        public void InterruptMatch_WithNoActiveMatch_ShouldReturnSuccess()
        {
            // Arrange
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()));
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, userLogic);
            MatchController SUT = new MatchController(matchLogic);

            // Act
            IActionResult result = SUT.InterruptMatch(1);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }
    }
}
