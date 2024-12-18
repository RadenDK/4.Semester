using FoosballProLeague.Api.Models;
using FoosballProLeague.Api.Models.RequestModels;
using Microsoft.AspNetCore.SignalR;
using System.Numerics;

namespace FoosballProLeague.Api.Hubs
{
    public class TableLoginHub : Hub
    {
        public async Task NotifyTableLogin(TableLoginRequest user)
        {
            await Clients.All.SendAsync("ReceiveTableLogin", user);
        }

        public async Task NotifyRemoveUser(string email)
        {
            await Clients.All.SendAsync("ReceiveRemoveUser", email);
        }
    }
}
