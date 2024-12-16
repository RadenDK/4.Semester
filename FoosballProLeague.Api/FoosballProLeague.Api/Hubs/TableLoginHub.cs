using FoosballProLeague.Api.Models;
using Microsoft.AspNetCore.SignalR;
using System.Numerics;

namespace FoosballProLeague.Api.Hubs
{
    public class TableLoginHub : Hub
    {
        public async Task NotifyTableLogin(UserModel user, string side)
        {
            await Clients.All.SendAsync("ReceiveTableLogin", user, side);
        }

        public async Task GetCurrentPlayers(UserModel user, string side)
        {
            await Clients.All.SendAsync("ReceiveCurrentUsers", user, side);
        }
    }
}
