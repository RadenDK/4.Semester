using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using FoosballProLeague.Api.Models.RequestModels;
using System.Linq.Expressions;
using FoosballProLeague.Api.BusinessLogic.Interfaces;

namespace FoosballProLeague.Api.Controllers
{
    [ApiController]
    //[Route("[controller]")]
    [ApiKeyAuthorize]
    public class MatchController : Controller
    {
        private readonly IMatchLogic _matchLogic;

        public MatchController(IMatchLogic matchLogic)
        {
            _matchLogic = matchLogic;
        }

        [HttpPost("LoginOnTable")]
        public IActionResult LoginOnTable([FromBody] TableLoginRequest tableLoginRequest)
        {
            try
            {
                if (_matchLogic.LoginOnTable(tableLoginRequest))
                {
                    return Ok("Login on table was successful.");
                }
                else
                {
                    return BadRequest("Login on table was not successful.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred: " + ex.Message);
            }
        }

        [HttpPost("StartMatch")]
        public IActionResult StartMatch(int tableId)
        {
            try
            {
                if (_matchLogic.StartMatch(tableId))
                {
                    return Ok("Starting match was successful.");
                }
                else
                {
                    return BadRequest("Starting match was not successful.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred: " + ex.Message);
            }
        }

        [HttpPost("InterruptMatch")]
        public IActionResult InterruptMatch(int tableId)
        {
            try
            {
                _matchLogic.InterruptMatch(tableId);

                return Ok("Interrupting match was successful.");

            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred: " + ex.Message);
            }
        }

        [HttpPost("RegisterGoal")]
        public IActionResult RegisterGoal([FromBody] RegisterGoalRequest registerGoalRequest)
        {
            try
            {
                if (_matchLogic.RegisterGoal(registerGoalRequest))
                {
                    return Ok("Registering goal was successful.");
                }
                else
                {
                    return BadRequest("Registering goal was not successful.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred: " + ex.Message);
            }
        }
    }
}
