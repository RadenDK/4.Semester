using FoosballProLeague.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using FoosballProLeague.Api.DatabaseAccess;


namespace FoosballProLeague.Api.BusinessLogic;



public class LeaderboardService
{
    private readonly IHubContext<LeaderboardHub> _hubContext;
    private readonly IUserDatabaseAccessor _userDatabaseAccessor;

    public LeaderboardService(IHubContext<LeaderboardHub> hubContext, IUserDatabaseAccessor userDatabaseAccessor)
    {
        _hubContext = hubContext;
        _userDatabaseAccessor = userDatabaseAccessor;
    }

    public async Task UpdateLeaderboard()
    {
        var users = _userDatabaseAccessor.GetUsers();
        var leaderboard = users.OrderByDescending(u => u.Elo1v1).ToList();
        await _hubContext.Clients.All.SendAsync("ReceiveLeaderboardUpdate", leaderboard);
    }

    public async Task NotifyLeaderboardChange()
    {
        await UpdateLeaderboard();
    }
}