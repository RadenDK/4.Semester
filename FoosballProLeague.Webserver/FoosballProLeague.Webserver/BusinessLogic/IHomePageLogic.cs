using FoosballProLeague.Webserver.Models;

namespace FoosballProLeague.Webserver.BusinessLogic
{
    public interface IHomePageLogic
    {
        public Task<List<MatchHistoryModel>> GetMatchHistoryByUserId(int userId);
        public Task<HomePageViewModel> GetUsersAndMatchHistory(string mode);
        public string GetTimeAgo(string endTime);
        Task<List<UserModel>> GetLeaderboards(string mode, int pageNumber, int pageSize);

    }
}
