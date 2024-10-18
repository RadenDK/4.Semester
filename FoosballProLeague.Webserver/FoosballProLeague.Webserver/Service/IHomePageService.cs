using FoosballProLeague.Webserver.Models;

namespace FoosballProLeague.Webserver.Service
{
    public interface IHomePageService
    {
        Task<List<UserModel>> GetUsers();
    }
}
