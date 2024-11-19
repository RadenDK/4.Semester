using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.BusinessLogic.Interfaces;
using FoosballProLeague.Api.Controllers;
using FoosballProLeague.Api.DatabaseAccess;
using FoosballProLeague.Api.DatabaseAccess.Interfaces;
using FoosballProLeague.Api.Models.DbModels;
using FoosballProLeague.Api.Models.RequestModels;
using Microsoft.AspNetCore.Mvc;

namespace FoosballProLeague.Api.Tests.ControllerTests.IntergrationTests.MatchControllerTests
{
    [Collection("Non-Parallel Database Collection")]
    public class CompleteGameTests : DatabaseTestBase
    {
        [Fact]
        public void Test1vs1MatchFinishes_2vs2Matchfinshes_1vs1WithSamePlayersFromFirstMatchShouldBeAbleToStart()
        {
            // Arrange

            _dbHelper.InsertData("INSERT INTO users (id) VALUES (1), (2), (3), (4)");
            _dbHelper.InsertData("INSERT INTO foosball_tables (id) VALUES (1), (2)");

            IUserDatabaseAccessor userDatabaseAccessor = new UserDatabaseAccessor(_dbHelper.GetConfiguration());
            ITeamDatabaseAccessor teamDatabaseAccessor = new TeamDatabaseAccessor(_dbHelper.GetConfiguration(), userDatabaseAccessor);
            IMatchDatabaseAccessor matchDatabaseAccessor = new MatchDatabaseAccessor(_dbHelper.GetConfiguration(), teamDatabaseAccessor);

            IUserLogic userLogic = new UserLogic(userDatabaseAccessor);
            IMatchLogic matchLogic = new MatchLogic(matchDatabaseAccessor, userLogic, teamDatabaseAccessor);

            MatchController SUT = new MatchController(matchLogic);

            TableLoginRequest loginUser1 = new TableLoginRequest
            {
                UserId = 1,
                TableId = 1,
                Side = "red"
            };
            TableLoginRequest loginUser2 = new TableLoginRequest
            {
                UserId = 2,
                TableId = 1,
                Side = "blue"
            };
            TableLoginRequest loginUser3 = new TableLoginRequest
            {
                UserId = 3,
                TableId = 1,
                Side = "red"
            };
            TableLoginRequest loginUser4 = new TableLoginRequest
            {
                UserId = 4,
                TableId = 1,
                Side = "blue"
            };

            List<IActionResult> loginOnTableResults = new List<IActionResult>();
            List<IActionResult> startMatchResults = new List<IActionResult>();
            List<IActionResult> registerGoalResults = new List<IActionResult>();

            // Act

            // First it is a complete 1vs1 match with user 1 against user 2
            loginOnTableResults.Add(SUT.LoginOnTable(loginUser1));
            loginOnTableResults.Add(SUT.LoginOnTable(loginUser2));

            startMatchResults.Add(SUT.StartMatch(1));

            for (int i = 0; i < 10; i++)
            {
                registerGoalResults.Add(SUT.RegisterGoal(new RegisterGoalRequest { TableId = 1, Side = "red" }));
            }

            // Second is a complete 2vs2 match with user 1 and user 3 against user 2 and user 4
            loginOnTableResults.Add(SUT.LoginOnTable(loginUser1));
            loginOnTableResults.Add(SUT.LoginOnTable(loginUser2));
            loginOnTableResults.Add(SUT.LoginOnTable(loginUser3));
            loginOnTableResults.Add(SUT.LoginOnTable(loginUser4));

            startMatchResults.Add(SUT.StartMatch(1));

            for (int i = 0; i < 10; i++)
            {
                registerGoalResults.Add(SUT.RegisterGoal(new RegisterGoalRequest { TableId = 1, Side = "red" }));
            }

            // Third is a 1vs1 match with user 1 against user 2 again
            loginOnTableResults.Add(SUT.LoginOnTable(loginUser1));
            loginOnTableResults.Add(SUT.LoginOnTable(loginUser2));

            startMatchResults.Add(SUT.StartMatch(1));

            for (int i = 0; i < 10; i++)
            {
                registerGoalResults.Add(SUT.RegisterGoal(new RegisterGoalRequest { TableId = 1, Side = "red" }));
            }

            // Assert

            foreach(IActionResult result in loginOnTableResults)
            {
                Assert.IsType<OkObjectResult>(result);
            }
            foreach(IActionResult result in startMatchResults)
            {
                Assert.IsType<OkObjectResult>(result);
            }
            foreach(IActionResult result in registerGoalResults)
            {
                Assert.IsType<OkObjectResult>(result);
            }

            IEnumerable<MatchDbModel> dbMatches = _dbHelper.ReadData<MatchDbModel>("SELECT * FROM foosball_matches");

            Assert.True (dbMatches.Count() == 3);

            IEnumerable<TeamDbModel> dbTeams = _dbHelper.ReadData<TeamDbModel>("SELECT * FROM teams");

            Assert.True(dbTeams.Count() == 4);
        }
    }
}
