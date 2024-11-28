using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.BusinessLogic.Interfaces;
using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.Hubs;
using FoosballProLeague.Api.Models;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace FoosballProLeague.Api.Tests.ControllerTests.IntergrationTests.SignalRTests
{
    [Collection("Non-Parallel Database Collection")]
    public class LeaderboardTests : DatabaseTestBase
    {
        [Fact]
        public async Task LeaderboardUpdate_1v1_ShouldSendCorrectData()
        {
            // Arrange: Set up the test data and mock objects
            int mockPlayer1Id = 1;
            int mockPlayer2Id = 2;
            _dbHelper.InsertData($"INSERT INTO users (id, elo_1v1, elo_2v2) VALUES ({mockPlayer1Id}, 1000, 1500), ({mockPlayer2Id}, 1200, 1400)");

            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();
            Mock<IHubClients> mockClients = new Mock<IHubClients>();
            Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();

            mockHubContext.Setup(hub => hub.Clients).Returns(mockClients.Object);
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHubContext.Object);

            // Act: Call the method to be tested
            await userLogic.UpdateLeaderboard("1v1");

            // Assert: Verify the expected outcomes for 1v1
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

        [Fact]
        public async Task LeaderboardUpdate_2v2_ShouldSendCorrectData()
        {
            // Arrange: Set up the test data and mock objects
            int mockPlayer1Id = 1;
            int mockPlayer2Id = 2;
            _dbHelper.InsertData($"INSERT INTO users (id, elo_1v1, elo_2v2) VALUES ({mockPlayer1Id}, 1000, 1500), ({mockPlayer2Id}, 1200, 1400)");

            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();
            Mock<IHubClients> mockClients = new Mock<IHubClients>();
            Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();

            mockHubContext.Setup(hub => hub.Clients).Returns(mockClients.Object);
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHubContext.Object);

            // Act: Call the method to be tested
            await userLogic.UpdateLeaderboard("2v2");

            // Assert: Verify the expected outcomes for 2v2
            mockClientProxy.Verify(
                client => client.SendCoreAsync(
                    "ReceiveLeaderboardUpdate",
                    It.Is<object[]>(o => o != null && o.Length == 1 &&
                                         ((List<UserModel>)o[0]).Count == 2 &&
                                         ((List<UserModel>)o[0]).Any(u => u.Id == mockPlayer1Id && u.Elo2v2 == 1500) &&
                                         ((List<UserModel>)o[0]).Any(u => u.Id == mockPlayer2Id && u.Elo2v2 == 1400)),
                    default),
                Times.Once);
        }
    }
}