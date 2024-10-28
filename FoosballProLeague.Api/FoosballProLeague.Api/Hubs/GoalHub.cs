using Microsoft.AspNetCore.SignalR;
using FoosballProLeague.Api.Models.FoosballModels;

namespace FoosballProLeague.Api.Hubs
{
    public class GoalHub : Hub
    {

        public async Task NotifyGoalsScored(TeamNikoModel teamRed, TeamNikoModel teamBlue, int redScore, int blueScore)
        {
            await Clients.All.SendAsync("RecieveGoalUpdate", teamRed, teamBlue, redScore, blueScore);
        }
    }
}
