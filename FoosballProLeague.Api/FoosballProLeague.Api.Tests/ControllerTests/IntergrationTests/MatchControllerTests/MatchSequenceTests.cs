using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.Hubs;
using FoosballProLeague.Api.Models.RequestModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoosballProLeague.Api.Tests.ControllerTests.IntergrationTests.MatchControllerTests
{
    [Collection("Non-Parallel Database Collection")]
    public class MatchSequenceTests : DatabaseTestBase
    {

        [Fact]
        public void PlayersLogin_StartAMatch_ScoreSomeGoalsUntillTheMatchIsOver_ShouldReturnSuccessAllTheWay()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id, elo_1v1, elo_2v2) VALUES (1, 500, 500), (2, 500, 500);");
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1);");

            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();
            Mock<IHubClients> mockClients = new Mock<IHubClients>();
            Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();

            mockHubContext.Setup(hub => hub.Clients).Returns(mockClients.Object);
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), (IHubContext<HomepageHub>)mockHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, (IHubContext<HomepageHub>)mockHubContext.Object, userLogic);
            MatchController SUT = new MatchController(matchLogic);

            TableLoginRequest tableLoginRequestUser1 = new TableLoginRequest
            {
                TableId = 1,
                UserId = 1,
                Side = "red"
            };
            TableLoginRequest tableLoginRequestUser2 = new TableLoginRequest
            {
                TableId = 1,
                UserId = 2,
                Side = "blue"
            };

            RegisterGoalRequest registerGoalRequestRedSide = new RegisterGoalRequest
            {
                TableId = 1,
                Side = "red"
            };
            // Act

            IActionResult loginUser1Result = SUT.LoginOnTable(tableLoginRequestUser1);
            IActionResult loginUser2Result = SUT.LoginOnTable(tableLoginRequestUser2);

            IActionResult startMatchResult = SUT.StartMatch(1);


            List<IActionResult> registerGoalResults = new List<IActionResult>();
            for (int i = 0; i < 10; i++)
            {
                registerGoalResults.Add(SUT.RegisterGoal(registerGoalRequestRedSide));
            }

            // Assert
            Assert.IsType<OkObjectResult>(loginUser1Result);
            Assert.IsType<OkObjectResult>(loginUser2Result);
            Assert.IsType<OkObjectResult>(startMatchResult);
            foreach (IActionResult result in registerGoalResults)
            {
                Assert.IsType<OkObjectResult>(result);
            }
        }

        [Fact]
        public void PlayersLoginPlayAMatchTillItsOverThenTriesToLoginAndPlayAgain_ShouldReturnSuccess()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id, elo_1v1, elo_2v2) VALUES (1, 500, 500), (2, 500, 500);");
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1);");

            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration());
            Mock<IHubContext<HomepageHub>> mockHubContext = new Mock<IHubContext<HomepageHub>>();
            Mock<IHubClients> mockClients = new Mock<IHubClients>();
            Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();

            mockHubContext.Setup(hub => hub.Clients).Returns(mockClients.Object);
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), (IHubContext<HomepageHub>)mockHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, (IHubContext<HomepageHub>)mockHubContext.Object, userLogic);
            MatchController SUT = new MatchController(matchLogic);

            TableLoginRequest tableLoginRequestUser1 = new TableLoginRequest
            {
                TableId = 1,
                UserId = 1,
                Side = "red"
            };
            TableLoginRequest tableLoginRequestUser2 = new TableLoginRequest
            {
                TableId = 1,
                UserId = 2,
                Side = "blue"
            };

            RegisterGoalRequest registerGoalRequestRedSide = new RegisterGoalRequest
            {
                TableId = 1,
                Side = "red"
            };
            // Act

            IActionResult loginUser1ResultFirst = SUT.LoginOnTable(tableLoginRequestUser1);
            IActionResult loginUser2ResultFirst = SUT.LoginOnTable(tableLoginRequestUser2);

            IActionResult startMatchResultFirst = SUT.StartMatch(1);


            List<IActionResult> registerGoalResults = new List<IActionResult>();
            for (int i = 0; i < 10; i++)
            {
                registerGoalResults.Add(SUT.RegisterGoal(registerGoalRequestRedSide));
            }

            IActionResult loginUser1ResultSecond = SUT.LoginOnTable(tableLoginRequestUser1);
            IActionResult loginUser2ResultSecond = SUT.LoginOnTable(tableLoginRequestUser2);

            IActionResult startMatchResultSecond = SUT.StartMatch(1);

            for (int i = 0; i < 10; i++)
            {
                registerGoalResults.Add(SUT.RegisterGoal(registerGoalRequestRedSide));
            }

            // Assert
            Assert.IsType<OkObjectResult>(loginUser1ResultSecond);
            Assert.IsType<OkObjectResult>(loginUser2ResultSecond);
            Assert.IsType<OkObjectResult>(startMatchResultSecond);
            foreach (IActionResult result in registerGoalResults)
            {
                Assert.IsType<OkObjectResult>(result);
            }
        }
    }
}
