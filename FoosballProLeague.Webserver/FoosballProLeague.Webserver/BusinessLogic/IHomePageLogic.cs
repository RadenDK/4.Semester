using FoosballProLeague.Webserver.Models;

namespace FoosballProLeague.Webserver.BusinessLogic
{
    public interface IHomePageLogic
    {
        Task<List<UserModel>> GetUsers();
    }
}
