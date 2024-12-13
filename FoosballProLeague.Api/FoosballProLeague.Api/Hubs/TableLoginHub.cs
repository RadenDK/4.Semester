using FoosballProLeague.Api.Models;
using Microsoft.AspNetCore.SignalR;

namespace FoosballProLeague.Api.Hubs
{
    public class TableLoginHub : Hub
    {
        public async Task NotifyTableLogin(UserModel user)
        {
            await Clients.All.SendAsync("ReceiveTableLogin", user);
        }
    }
}
