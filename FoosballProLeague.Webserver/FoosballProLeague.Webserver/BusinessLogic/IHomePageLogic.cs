using FoosballProLeague.Webserver.Models;

namespace FoosballProLeague.Webserver.BusinessLogic
{
    public interface IHomePageLogic
    {
        Task<List<UserModel>> GetLeaderboards(string mode, int pageNumber, int pageSize);
        Task<int> GetTotalUserCount(string mode);

    }
}
