using FoosballProLeague.Webserver.Models;
using FoosballProLeague.Webserver.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace FoosballProLeague.Webserver.BusinessLogic
{
    public class HomePageLogic : IHomePageLogic
    {
        private readonly IHomePageService _homePageService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomePageLogic(IHomePageService homePageService, IHttpContextAccessor httpContextAccessor)
        {
            _homePageService = homePageService;
            _httpContextAccessor = httpContextAccessor;
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

        public async Task<List<MatchHistoryModel>> GetMatchHistoryByUserId(int userId)
        {
            return await _homePageService.GetMatchHistoryByUserId(userId);
        }

        public async Task<HomePageViewModel> GetUsersAndMatchHistory(string mode)
        {
            try
            {
                List<UserModel> users = await GetAllUsers();
                UserModel user = GetUserFromCookie();
                List<MatchHistoryViewModel> matchHistory = null;

                if (user != null)
                {
                    try
                    {
                        List<MatchHistoryModel> matchHistoryModels = await GetMatchHistoryByUserId(user.Id);
                        matchHistory = matchHistoryModels
                            .OrderByDescending(m => DateTime.Parse(m.EndTime))
                            .Select(m => new MatchHistoryViewModel
                            {
                                RedTeamUser1 = m.RedTeam.User1.FirstName,
                                RedTeamUser2 = m.RedTeam.User2.FirstName,
                                BlueTeamUser1 = m.BlueTeam.User1.FirstName,
                                BlueTeamUser2 = m.BlueTeam.User2.FirstName,
                                RedTeamScore = m.RedTeamScore,
                                BlueTeamScore = m.BlueTeamScore,
                                TimeAgo = GetTimeAgo(m.EndTime)
                            }).ToList();
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException("An error occurred while retrieving match history.", ex);
                    }
                }

                HomePageViewModel viewModel = new HomePageViewModel
                {
                    Users = users,
                    MatchHistory = matchHistory,
                    Mode = mode
                };
                return viewModel;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while retrieving leaderboard.", ex);
            }


        }

        private UserModel GetUserFromCookie()
        {
            string cookieValue = _httpContextAccessor.HttpContext.Request.Cookies["User"];
            if (string.IsNullOrEmpty(cookieValue))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<UserModel>(cookieValue);
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
