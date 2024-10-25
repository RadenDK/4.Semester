using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoosballProLeague.Api.Tests.ControllerTests.IntergrationTests.MatchControllerTests
{
    public class ScoringTests
    {
        // Test: Goal scored on non-active table but with valid pending teams
        [Fact]
        public void GoalScored_OnNonActiveTableWithValidPendingTeams_ShouldCreateMatchAndRegisterGoal()
        {
            // Arrange

            // Act

            // Assert
        }

        // Test: Goal scored on table with no pending players
        [Fact]
        public void GoalScored_WithNoPendingPlayers_ShouldNotRegisterGoal()
        {
            // Arrange

            // Act

            // Assert
        }

        // Test: Goal scored on table with unbalanced teams (1 player on one side, 2 on the other)
        [Fact]
        public void GoalScored_WithUnbalancedTeams_ShouldNotStartMatchOrRegisterGoal()
        {
            // Arrange

            // Act

            // Assert
        }

        // Test: Enough goals scored that a side reaches 10 goals
        [Fact]
        public void GoalScored_WhenSideReaches10Goals_ShouldCompleteMatchAndDeclareWinner()
        {
            // Arrange

            // Act

            // Assert
        }
    }
}
