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

        public async Task<List<UserModel>> GetAllUsers()
        {
            return await _homePageService.GetAllUsers();
        }
    }
}
