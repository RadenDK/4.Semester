using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoosballProLeague.Api.Tests.ControllerTests.IntergrationTests.MatchControllerTests
{
    [Collection("Non-Parallel Database Collection")]
    public class InterruptMatchTests : DatabaseTestBase
    {
        [Fact]
        public void InterruptMatch_WithActiveMatch_ShouldReturnSuccess()
        {
            // Arrange

            // Act

            // Assert
        }

        [Fact]
        public void InterruptMatch_WithNoActiveMatch_ShouldReturnSuccess()
        {
            // Arrange

            // Act

            // Assert
        }
    }
}
