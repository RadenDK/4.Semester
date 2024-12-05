using FoosballProLeague.Webserver.Models;

namespace FoosballProLeague.Webserver.BusinessLogic.Interfaces
{
    public interface IHomePageLogic
    {
        public Task<List<MatchHistoryModel>> GetMatchHistoryByUserId(int userId);
        public Task<HomePageViewModel> GetUsersAndMatchHistory(string mode, int pageNumber, int pageSize);
        public string GetTimeAgo(string endTime);
        public Task<List<UserModel>> GetLeaderboards(string mode, int pageNumber, int pageSize);
        public Task<int> GetTotalUserCount(string mode);
    }
}
