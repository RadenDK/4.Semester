

using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.BusinessLogic.Interfaces;
using FoosballProLeague.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoosballProLeague.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[ApiKeyAuthorize]
    public class TournamentController : Controller
    {
        private readonly ITournamentLogic _tournamentLogic;

        public TournamentController(ITournamentLogic tournamentLogic)
        {
            _tournamentLogic = tournamentLogic;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTournament([FromBody] TournamentModel tournament)
        {
            bool result = await _tournamentLogic.CreateTournamentAsync(tournament);
            if (result)
            {
                return Ok("Tournament created succesfully.");
            }

            return BadRequest("Failed to create tournament.");
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinTournament(int tournamentId, int teamId)
        {
            bool result = await _tournamentLogic.JoinTournamentAsync(tournamentId, teamId);
            if (result)
            {
                return Ok("Joined tournament successfully.");
            }

            return BadRequest("Failed to join tournament.");
        }
    }
}