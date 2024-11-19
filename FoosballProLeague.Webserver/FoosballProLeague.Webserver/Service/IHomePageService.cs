using FoosballProLeague.Webserver.Models;

namespace FoosballProLeague.Webserver.Service
{
    public interface IHomePageService
    {
        public Task<List<MatchHistoryModel>> GetMatchHistoryByUserId(int userId);
        Task<List<UserModel>> GetAllUsers();
    }
}
