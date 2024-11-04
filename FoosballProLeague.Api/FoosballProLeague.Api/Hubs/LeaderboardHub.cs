namespace FoosballProLeague.Api.Hubs;

using Microsoft.AspNetCore.SignalR;
using FoosballProLeague.Api.Models;

public class LeaderboardHub : Hub
{
    public async Task UpdateLeaderboard(List<UserModel> leaderboard)
    {
        await Clients.All.SendAsync("ReceiveLeaderboardUpdate", leaderboard);
    }
}