using FoosballProLeague.Webserver.Models;

namespace FoosballProLeague.Webserver.BusinessLogic
{
    public interface IHomePageLogic
    {
        Task<List<UserModel>> GetUsers(int pageNumber, int pageSize);
        Task<int> GetTotalUserCount();

    }
}
