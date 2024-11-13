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

        public async Task<List<UserModel>> GetLeaderboards(string mode, int pageNumber, int pageSize)
        {
            
            Dictionary<string, List<UserModel>> usersDictionary = await _homePageService.GetLeaderboards();
            if (usersDictionary.TryGetValue(mode, out List<UserModel> users))
            {
                return users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                throw new Exception($"Mode {mode} not found in the leaderboards.");
            }
        }

        public async Task<int> GetTotalUserCount(string mode)
        {
            Dictionary<string, List<UserModel>> usersDict = await _homePageService.GetLeaderboards();
            if (usersDict.TryGetValue(mode, out List<UserModel> users))
            {
                return users.Count;
            }
            else
            {
                throw new Exception($"Mode {mode} not found in the leaderboards.");
            }
        }
    }
}
