﻿using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.BusinessLogic.Interfaces;
using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.DatabaseAccess.Interfaces;
using FoosballProLeague.Api.Hubs;
using FoosballProLeague.Api.Models.RequestModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace FoosballProLeague.Api.Tests.ControllerTests.IntergrationTests.MatchControllerTests
{
    [Collection("Non-Parallel Database Collection")]
    public class MatchSequenceTests : DatabaseTestBase
    {

        [Fact]
        public void UsersLogin_StartAMatch_ScoreSomeGoalsUntillTheMatchIsOver_ShouldReturnSuccessAllTheWay()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id, email, elo_1v1, elo_2v2) VALUES (1, 'user1@email.com', 500, 500), (2, 'user2@email.com', 500, 500);");
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1);");

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);
            Mock<IHubContext<HomepageHub>> mockHomepageHubContext = new Mock<IHubContext<HomepageHub>>();
            Mock<IHubContext<TableLoginHub>> mockTableLoginHubContext = new Mock<IHubContext<TableLoginHub>>();

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHomepageHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, mockHomepageHubContext.Object, mockTableLoginHubContext.Object, userLogic, teamDatabaseAccessor);
            
            MatchController SUT = new MatchController(matchLogic);

            TableLoginRequest tableLoginRequestUser1 = new TableLoginRequest
            {
                TableId = 1,
                Email = "user1@email.com",
                UserId = 1,
                Side = "red"
            };
            TableLoginRequest tableLoginRequestUser2 = new TableLoginRequest
            {
                TableId = 1,
                Email = "user2@email.com",
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
        public void UsersLoginPlayAMatchTillItsOverThenTriesToLoginAndPlayAgain_ShouldReturnSuccess()
        {
            // Arrange
            _dbHelper.InsertData("INSERT INTO users (id, email, elo_1v1, elo_2v2) VALUES (1, 'user1@email.com', 500, 500), (2, 'user2@email.com', 500, 500);");
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1);");

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);
            Mock<IHubContext<HomepageHub>> mockHomepageHubContext = new Mock<IHubContext<HomepageHub>>();
            Mock<IHubContext<TableLoginHub>> mockTableLoginHubContext = new Mock<IHubContext<TableLoginHub>>();

            IUserLogic userLogic = new UserLogic(new UserDatabaseAccessor(_dbHelper.GetConfiguration()), mockHomepageHubContext.Object);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, mockHomepageHubContext.Object, mockTableLoginHubContext.Object, userLogic, teamDatabaseAccessor);
            MatchController SUT = new MatchController(matchLogic);

            TableLoginRequest tableLoginRequestUser1 = new TableLoginRequest
            {
                TableId = 1,
                Email = "user1@email.com",
                UserId = 1,
                Side = "red"
            };
            TableLoginRequest tableLoginRequestUser2 = new TableLoginRequest
            {
                TableId = 1,
                Email = "user2@email.com",
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
