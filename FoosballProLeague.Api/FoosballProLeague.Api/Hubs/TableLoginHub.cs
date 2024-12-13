using FoosballProLeague.Api.Models;
using Microsoft.AspNetCore.SignalR;
using System.Numerics;

namespace FoosballProLeague.Api.Hubs
{
    public class TableLoginHub : Hub
    {
        public async Task NotifyTableLogin(UserModel user)
        {
            await Clients.All.SendAsync("ReceiveTableLogin", user);
        }

        public async Task GetCurrentPlayers(UserModel user)
        {
            await Clients.All.SendAsync("ReceiveCurrentUsers", user);
        }
    }
}
