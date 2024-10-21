using Xunit;

namespace FoosballProLeague.Api.Tests
{
	public class TableLoginTests
	{
		// Test: Login when the table is empty with one player
		[Fact]
		public void Login_WhenTableIsEmpty_ShouldRegisterOnePlayer()
		{
			// Arrange

			// Act

			// Assert
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
