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

        public async Task<List<UserModel>> GetUsers()
        {
            return await _homePageService.GetUsers();
        }

        public async Task<List<MatchHistoryModel>> GetMatchHistoryByUserId(int userId)
        {
            return await _homePageService.GetMatchHistoryByUserId(userId);
        }
        
        public string GetTimeAgo(string endTime)
        {
            DateTime endDateTime = DateTime.Parse(endTime);
            TimeSpan timeSpan = DateTime.Now - endDateTime;

            if (timeSpan.TotalMinutes < 1)
            {
                return "Just now";
            }
            else if (timeSpan.TotalMinutes < 60)
            {
                return $"{(int)timeSpan.TotalMinutes} minutes ago";
            }
            else if (timeSpan.TotalHours < 24)
            {
                return $"{(int)timeSpan.TotalHours} hours ago";
            }
            else if (timeSpan.TotalDays < 30)
            {
                return $"{(int)timeSpan.TotalDays} days ago";
            }
            else if (timeSpan.TotalDays < 365)
            {
                return $"{(int)(timeSpan.TotalDays / 30)} months ago";
            }
            else
            {
                return $"{(int)(timeSpan.TotalDays / 365)} years ago";
            }
        }
    }
}
