using Microsoft.AspNetCore.SignalR;
using FoosballProLeague.Api.Models.FoosballModels;

namespace FoosballProLeague.Api.Hubs
{
    public class MatchHub : Hub
    {
        public async Task NotifyMatchEnd(bool isMatchStart)
        {
            await Clients.All.SendAsync("RecieveMatchEnd", isMatchStart);
        }

        public async Task NotifyMatchStart(bool isMatchStart, TeamModel teamRed, TeamModel teamBlue, int redScore, int blueScore)
        {
            await Clients.All.SendAsync("RecieveMatchStart", isMatchStart, teamRed, teamBlue, redScore, blueScore);
        }

        public async Task NotifyGoalsScored(TeamModel teamRed, TeamModel teamBlue, int redScore, int blueScore)
        {
            await Clients.All.SendAsync("RecieveGoalUpdate", teamRed, teamBlue, redScore, blueScore);
        }
    }
}
