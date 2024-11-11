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
            // Arrange
            int mockPlayer1Id = 1;
            int mockPlayer2Id = 2;

            _dbHelper.InsertData($"INSERT INTO users (id, elo_1v1) VALUES ({mockPlayer1Id}, 1000), ({mockPlayer2Id}, 1200)");

            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();
            Mock<IHubClients> mockClients = new Mock<IHubClients>();
            Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();

            mockHubContext.Setup(hub => hub.Clients).Returns(mockClients.Object);
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHubContext.Object);

            // Act
            await userLogic.UpdateLeaderboard();

            // Assert
            IEnumerable<UserModel> users = _dbHelper.ReadData<UserModel>("SELECT * FROM users");

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