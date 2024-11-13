using FoosballProLeague.Webserver.Models;

namespace FoosballProLeague.Webserver.Service
{
    public interface IHomePageService
    {
        Task<Dictionary<string, List<UserModel>>> GetLeaderboards();
    }
}
