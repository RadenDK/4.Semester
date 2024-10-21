using Microsoft.AspNetCore.Mvc;
using FoosballProLeague.Api.BusinessLogic;
using System.Runtime.CompilerServices;
using FoosballProLeague.Api.Models.RequestModels;

namespace FoosballProLeague.Api.Controllers
{
    public class MatchController : Controller
    {

        private IMatchLogic _matchLogic;

        [HttpPost("LoginOnTable")]
        public IActionResult LoginOnTable([FromBody] TableLoginRequest tableLoginRequest)
        {
            if (_matchLogic.LoginOnTable(tableLoginRequest))
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost("RegisterGoal")]
        public IActionResult RegisterGoal(RegisterGoalRequest registerGoalRequest)
        {
            if (_matchLogic.RegisterGoal(registerGoalRequest))
            {
                return Ok();

            }
            else
            {
                return BadRequest();
            }
        }
    }
}
