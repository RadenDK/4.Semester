using Xunit;
using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.BusinessLogic;
using Microsoft.Extensions.Configuration;
using FoosballProLeague.Api.Models.RequestModels;
using FoosballProLeague.Api.Models.FoosballModels;
using FoosballProLeague.Api.DatabaseAccess;
using Microsoft.AspNetCore.Mvc;

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
            TableLoginRequest mockTableLoginRequest = new TableLoginRequest { PlayerId = 1, TableId = mockTableId, Side = "red" };

            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor);
            MatchController SUT = new MatchController(matchLogic);

            // Act
            SUT.LoginOnTable(mockTableLoginRequest);

            // Assert
            IEnumerable<MatchModel> football_matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> football_table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables where id = " + mockTableId);

            Assert.Empty(football_matches);
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
            TableLoginRequest mockTableLoginRequest1 = new TableLoginRequest { PlayerId = 1, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequest2 = new TableLoginRequest { PlayerId = 2, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequest3 = new TableLoginRequest { PlayerId = 3, TableId = mockTableId, Side = "blue" };
            TableLoginRequest mockTableLoginRequest4 = new TableLoginRequest { PlayerId = 4, TableId = mockTableId, Side = "blue" };

            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor);
            MatchController SUT = new MatchController(matchLogic);

            // Act
            SUT.LoginOnTable(mockTableLoginRequest1);
            SUT.LoginOnTable(mockTableLoginRequest2);
            SUT.LoginOnTable(mockTableLoginRequest3);
            SUT.LoginOnTable(mockTableLoginRequest4);

            // Assert
            IEnumerable<MatchModel> football_matches = _dbHelper.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> football_table = _dbHelper.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables where id = " + mockTableId);

            Assert.Empty(football_matches);
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
            TableLoginRequest mockTableLoginRequest1 = new TableLoginRequest { PlayerId = 1, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequest2 = new TableLoginRequest { PlayerId = 2, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequest3 = new TableLoginRequest { PlayerId = 3, TableId = mockTableId, Side = "red" };

            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor);
            MatchController SUT = new MatchController(matchLogic);

            // Act
            SUT.LoginOnTable(mockTableLoginRequest1); // First player logs in on the red side
            SUT.LoginOnTable(mockTableLoginRequest2); // Second player logs in on the red side
            IActionResult result = SUT.LoginOnTable(mockTableLoginRequest3); // Third player tries to log in on the red side

            // Assert
            Assert.IsType<BadRequestResult>(result); // Expect the third login to return a BadRequestResult
        }


        // Test: Login on full table (both teams have two players)
        [Fact]
        public void Login_WhenTableIsFull_ShouldNotAllowAdditionalPlayers()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id) VALUES (1), (2), (3), (4), (5), (6)");
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");

            int mockTableId = 1;
            TableLoginRequest mockTableLoginRequest1 = new TableLoginRequest { PlayerId = 1, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequest2 = new TableLoginRequest { PlayerId = 2, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequest3 = new TableLoginRequest { PlayerId = 3, TableId = mockTableId, Side = "blue" };
            TableLoginRequest mockTableLoginRequest4 = new TableLoginRequest { PlayerId = 4, TableId = mockTableId, Side = "blue" };
            TableLoginRequest mockTableLoginRequest5 = new TableLoginRequest { PlayerId = 5, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequest6 = new TableLoginRequest { PlayerId = 6, TableId = mockTableId, Side = "blue" };


            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor);
            MatchController SUT = new MatchController(matchLogic);

            // Act
            SUT.LoginOnTable(mockTableLoginRequest1); // First red player
            SUT.LoginOnTable(mockTableLoginRequest2); // Second red player
            SUT.LoginOnTable(mockTableLoginRequest3); // First blue player
            SUT.LoginOnTable(mockTableLoginRequest4); // Second blue player
            IActionResult result1 = SUT.LoginOnTable(mockTableLoginRequest5); // Third red player attempts to log in
            IActionResult result2 = SUT.LoginOnTable(mockTableLoginRequest6); // Third blue player attempts to log in


            // Assert
            Assert.IsType<BadRequestResult>(result1); // Expect the third red login to return a BadRequestResult
            Assert.IsType<BadRequestResult>(result2); // Expect the third blue login to return a BadRequestResult

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
            TableLoginRequest mockTableLoginRequest = new TableLoginRequest { PlayerId = 5, TableId = mockTableId, Side = "red" }; // Attempt to join the red side

            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor);
            MatchController SUT = new MatchController(matchLogic);

            // Act
            IActionResult result = SUT.LoginOnTable(mockTableLoginRequest); // Player tries to log in during an active match

            // Assert
            Assert.IsType<BadRequestResult>(result); // Expect the login to fail since the match is active and the red team is full
        }

        // Test: Login allowed when match is active but there is room on the team
        [Fact]
        public void Login_WhenMatchIsActiveAndTeamHasRoom_ShouldAllowLogin()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id) VALUES (1), (2), (3)");
            _dbHelper.InsertData("INSERT INTO teams (player1_id) VALUES (1), (2)"); 
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");
            _dbHelper.InsertData("INSERT INTO foosball_matches (id, table_id, red_team_id, blue_team_id) VALUES (1, 1, 1, 2)"); // Active match
            _dbHelper.UpdateData("UPDATE foosball_tables SET active_match_id = 1 WHERE id = 1");

            int mockTableId = 1;
            TableLoginRequest mockTableLoginRequest = new TableLoginRequest { PlayerId = 3, TableId = mockTableId, Side = "red" }; // Attempt to join the red side

            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor);
            MatchController SUT = new MatchController(matchLogic);

            // Act
            IActionResult result = SUT.LoginOnTable(mockTableLoginRequest); // Player tries to log in during an active match

            // Assert
            Assert.IsType<OkResult>(result); // Expect the login to succeed since there is room on the red team
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
            TableLoginRequest mockTableLoginRequest = new TableLoginRequest { PlayerId = 3, TableId = mockTableId, Side = "red" }; // Attempt to join the red side

            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor);
            MatchController SUT = new MatchController(matchLogic);

            // Act
            IActionResult result = SUT.LoginOnTable(mockTableLoginRequest); // Player tries to log in during an active match

            // Assert
            Assert.IsType<OkResult>(result); // Expect the login to succeed since there is room on the red team
        }
    }
}
