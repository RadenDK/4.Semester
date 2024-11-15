using FoosballProLeague.Webserver.Models;

namespace FoosballProLeague.Webserver.BusinessLogic
{
    public interface IHomePageLogic
    {
        Task<List<UserModel>> GetUsers();
        public Task<List<MatchHistoryModel>> GetMatchHistoryByUserId(int userId);
        public string GetTimeAgo(string endTime);
    }
}
