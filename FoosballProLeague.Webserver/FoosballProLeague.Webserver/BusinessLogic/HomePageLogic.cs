using FoosballProLeague.Webserver.Models;
using FoosballProLeague.Webserver.Service;

namespace FoosballProLeague.Webserver.BusinessLogic
{
    public class HomePageLogic : IHomePageLogic
    {
        private readonly IHomePageService _homePageService;

        public HomePageLogic(IHomePageService homePageService)
        {
            _homePageService = homePageService;
        }

        public async Task<List<UserModel>> GetUsers(int pageNumber, int pageSize)
        {
            var users = await _homePageService.GetUsers();
            return users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        }

        public async Task<int> GetTotalUserCount()
        {
            var users = await _homePageService.GetUsers();
            return users.Count;
        }
    }
}
