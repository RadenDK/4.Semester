using Xunit;
using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.BusinessLogic;
using System.IO;
using Microsoft.Extensions.Configuration;
using FoosballProLeague.Api.Models.RequestModels;
using FoosballProLeague.Api.Models.FoosballModels;
using FoosballProLeague.Api.DatabaseAccess;

namespace FoosballProLeague.Api.Tests
{
    // SUT = System Under Test
    public class TableLoginTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;

        public TableLoginTests(DatabaseFixture fixture)
        {
            _fixture = fixture; // Get the fixture injected automatically
        }

        // Test: Login when the table is empty with one player
        [Fact]
        public void Login_WhenTableIsEmpty_ShouldRegisterOnePlayer()
        {
            // Arrange
            _fixture.InsertData("INSERT INTO users (id) VALUES (1)");
            _fixture.InsertData("INSERT INTO foosball_tables (id) VALUES (1)");

            TableLoginRequest mockTableLoginRequest = new TableLoginRequest { PlayerId = 1, TableId = 1, Side = "red" };

            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_fixture.GetConfiguration());
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor);
            MatchController SUT = new MatchController(matchLogic);


            // Act
            SUT.LoginOnTable(mockTableLoginRequest);

            // Assert
            IEnumerable<MatchModel> football_matches = _fixture.ReadData<MatchModel>("SELECT * FROM foosball_matches");
            IEnumerable<FoosballTableModel> football_table = _fixture.ReadData<FoosballTableModel>("SELECT * FROM foosball_tables where id = " + mockTableLoginRequest.TableId);

            Assert.Empty(football_matches);
            Assert.True(football_table.First().ActiveMatchId == null);

        }

        // Test: Multiple logins on a table that was initially empty
        [Fact]
        public void Login_MultiplePlayersOnEmptyTable_ShouldRegisterMultiplePlayers()
        {
            // Arrange

            // Act

            // Assert
        }

        // Test: Login on full team (a team with two players)
        [Fact]
        public void Login_WhenTeamIsFull_ShouldNotAllowAdditionalPlayers()
        {
            // Arrange

            // Act

            // Assert
        }

        // Test: Login on full table (both teams have two players)
        [Fact]
        public void Login_WhenTableIsFull_ShouldNotAllowAdditionalPlayers()
        {
            // Arrange

            // Act

            // Assert
        }

        // Test: Login on table with an active match
        [Fact]
        public void Login_WhenMatchIsActive_ShouldNotAllowNewLogins()
        {
            // Arrange

            // Act

            // Assert
        }
    }
}
