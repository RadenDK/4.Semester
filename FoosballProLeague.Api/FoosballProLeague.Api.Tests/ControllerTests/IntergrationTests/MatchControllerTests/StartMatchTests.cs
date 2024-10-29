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
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor);
            MatchController SUT = new MatchController(matchLogic);

            TableLoginRequest mockTableLoginRequestPlayer1 = new TableLoginRequest { PlayerId = mockPlayer1Id, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequestPlayer2 = new TableLoginRequest { PlayerId = mockPlayer2Id, TableId = mockTableId, Side = "blue" };

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
            Assert.True(teams.First().Player1Id == 1 && teams.First().Player2Id == null);
            Assert.True(teams.Last().Player1Id == 2 && teams.Last().Player2Id == null);
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
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor);
            MatchController SUT = new MatchController(matchLogic);

            TableLoginRequest mockTableLoginRequestPlayer1 = new TableLoginRequest { PlayerId = mockPlayer1Id, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequestPlayer2 = new TableLoginRequest { PlayerId = mockPlayer2Id, TableId = mockTableId, Side = "blue" };
            TableLoginRequest mockTableLoginRequestPlayer3 = new TableLoginRequest { PlayerId = mockPlayer3Id, TableId = mockTableId, Side = "blue" };

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
            Assert.True(matches.First().RedTeamId == teams.First(t => t.Player1Id == mockPlayer1Id).Id);
            Assert.True(matches.First().BlueTeamId == teams.First(t => t.Player1Id == mockPlayer2Id && t.Player2Id == mockPlayer3Id).Id);
            Assert.True(matches.First().TeamRedScore == 0 && matches.First().TeamBlueScore == 0);
            Assert.True(matches.First().StartTime.AddSeconds(5) >= DateTime.Now);
            Assert.True(matches.First().EndTime == null);

            Assert.True(table.First().ActiveMatchId == matches.First().Id);

            Assert.True(teams.Count() == 2);
            Assert.True(teams.First(t => t.Player1Id == mockPlayer1Id).Player2Id == null);
            Assert.True(teams.First(t => t.Player1Id == mockPlayer2Id && t.Player2Id == mockPlayer3Id) != null);
        }

        [Fact]
        public void StartMatch_WithNoPendingTeams_ShouldReturnBadRequest()
        {
            // Arrange
            int mockTableId = 1;

            _dbHelper.InsertData($"INSERT INTO foosball_tables (id) VALUES ({mockTableId})");

            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor);
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
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor);
            MatchController SUT = new MatchController(matchLogic);

            TableLoginRequest mockTableLoginRequestPlayer1 = new TableLoginRequest { PlayerId = mockPlayer1Id, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequestPlayer2 = new TableLoginRequest { PlayerId = mockPlayer2Id, TableId = mockTableId, Side = "blue" };

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

            TeamModel redTeam = teams.FirstOrDefault(t => t.Player1Id == mockPlayer1Id && t.Player2Id == null);
            Assert.NotNull(redTeam);

            TeamModel blueTeam = teams.FirstOrDefault(t => t.Player1Id == mockPlayer2Id && t.Player2Id == null);
            Assert.NotNull(blueTeam);

            Assert.Equal(redTeam.Id, createdMatch.RedTeamId);
            Assert.Equal(blueTeam.Id, createdMatch.BlueTeamId);
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
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor);
            MatchController SUT = new MatchController(matchLogic);

            TableLoginRequest mockTableLoginRequestPlayer1 = new TableLoginRequest { PlayerId = 1, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequestPlayer2 = new TableLoginRequest { PlayerId = 2, TableId = mockTableId, Side = "blue" };

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
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor);
            MatchController SUT = new MatchController(matchLogic);

            TableLoginRequest mockTableLoginRequestPlayer1 = new TableLoginRequest { PlayerId = 1, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequestPlayer2 = new TableLoginRequest { PlayerId = 2, TableId = mockTableId, Side = "blue" };

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
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor);
            MatchController SUT = new MatchController(matchLogic);

            TableLoginRequest mockTableLoginRequestPlayer1 = new TableLoginRequest { PlayerId = mockPlayer1Id, TableId = mockTableId, Side = "red" };
            TableLoginRequest mockTableLoginRequestPlayer2 = new TableLoginRequest { PlayerId = mockPlayer2Id, TableId = mockTableId, Side = "blue" };
            TableLoginRequest mockTableLoginRequestPlayer3 = new TableLoginRequest { PlayerId = mockPlayer3Id, TableId = mockTableId, Side = "blue" };

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

            TeamModel newTeam = teams.FirstOrDefault(t => t.Player1Id == 2 && t.Player2Id == 3);
            Assert.NotNull(newTeam);

            Assert.Equal(existingTeam.Id, createdMatch.RedTeamId);
            Assert.Equal(newTeam.Id, createdMatch.BlueTeamId);
        }
    }
}
