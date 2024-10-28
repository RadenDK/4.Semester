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

            // Act

            // Assert
        }

        [Fact]
        public void StartMatch_WithInvarianceInTeamSize_ShouldReturnSuccess()
        {
            // Arrange

            // Act

            // Assert
        }

        [Fact]
        public void StartMatch_WithNoPendingTeams_ShouldReturnBadRequest()
        {
            // Arrange

            // Act

            // Assert
        }

        [Fact]
        public void StartMatch_ShouldCreateTeamIfNoExistingTeam()
        {
            // Arrange

            // Act

            // Assert
        }

        [Fact]
        public void StartMatch_ShouldNotCreateTeamIfExistingTeam()
        {
            // Arrange

            // Act

            // Assert
        }

        [Fact]
        public void StartMatch_WithActiveMatch_ShouldReturnBadRequest()
        {
            // Arrange

            // Act

            // Assert
        }
    }
}
