using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.Models;
using FoosballProLeague.Api.Models.FoosballModels;
using Microsoft.AspNetCore.SignalR;

namespace FoosballProLeague.Api.Hubs;

    public class HomepageHub : Hub
    {
        private readonly IUserLogic _userLogic;

        
        public HomepageHub(IUserLogic userLogic)
        {
            _userLogic = userLogic;
        }
        // Methods from LeaderboardHub
        public async Task UpdateLeaderboard(string mode)
        {
            await _userLogic.UpdateLeaderboard(mode);
        }

        // Methods from MatchHub
        public async Task NotifyMatchEnd(bool isMatchStart)
        {
            await Clients.All.SendAsync("ReceiveMatchEnd", isMatchStart);
        }

        public async Task NotifyMatchStart(bool isMatchStart, TeamModel teamRed, TeamModel teamBlue, int redScore, int blueScore)
        {
            await Clients.All.SendAsync("ReceiveMatchStart", isMatchStart, teamRed, teamBlue, redScore, blueScore);
        }

        public async Task NotifyGoalsScored(TeamModel teamRed, TeamModel teamBlue, int redScore, int blueScore)
        {
            await Clients.All.SendAsync("ReceiveGoalUpdate", teamRed, teamBlue, redScore, blueScore);
        }
    }

