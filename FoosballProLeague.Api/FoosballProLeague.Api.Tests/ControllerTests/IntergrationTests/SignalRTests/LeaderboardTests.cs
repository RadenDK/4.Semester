using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.Hubs;
using FoosballProLeague.Api.Models;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoosballProLeague.Api.Tests.ControllerTests.IntergrationTests.SignalRTests
{
    [Collection("Non-Parallel Database Collection")]
    public class LeaderboardTests : DatabaseTestBase
    {
        [Fact]
        public async Task LeaderboardUpdate_ShouldSendCorrectData()
        {
            // Arrange: Set up the test data and mock objects

            // Define mock player IDs
            int mockPlayer1Id = 1;
            int mockPlayer2Id = 2;

            // Insert mock data into the database for users with specific ELO ratings
            _dbHelper.InsertData($"INSERT INTO users (id, elo_1v1) VALUES ({mockPlayer1Id}, 1000), ({mockPlayer2Id}, 1200)");

            // Create mock objects for IHubContext, IHubClients, and IClientProxy
            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();
            Mock<IHubClients> mockClients = new Mock<IHubClients>();
            Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();

            // Set up the mockHubContext to return the mockClients object when Clients property is accessed
            mockHubContext.Setup(hub => hub.Clients).Returns(mockClients.Object);
            // Set up the mockClients to return the mockClientProxy object when All property is accessed
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            // Initialize the UserLogic with the mock database accessor and mock hub context
            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHubContext.Object);

            // Act: Call the method to be tested

            // Call the UpdateLeaderboard method to trigger the leaderboard update
            await userLogic.UpdateLeaderboard();

            // Assert: Verify the expected outcomes

            // Retrieve the users from the database to verify the data
            IEnumerable<UserModel> users = _dbHelper.ReadData<UserModel>("SELECT * FROM users");

            // Verify that the SendCoreAsync method was called once with the correct parameters
            mockClientProxy.Verify(
                client => client.SendCoreAsync(
                    "ReceiveLeaderboardUpdate",
                    It.Is<object[]>(o => o != null && o.Length == 1 &&
                                    ((List<UserModel>)o[0]).Count == 2 &&
                                    ((List<UserModel>)o[0]).Any(u => u.Id == mockPlayer1Id && u.Elo1v1 == 1000) &&
                                    ((List<UserModel>)o[0]).Any(u => u.Id == mockPlayer2Id && u.Elo1v1 == 1200)),
                default),
                Times.Once);
        }
    }
}