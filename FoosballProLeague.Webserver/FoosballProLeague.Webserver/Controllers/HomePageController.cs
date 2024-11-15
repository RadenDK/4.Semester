using FoosballProLeague.Webserver.BusinessLogic;
using FoosballProLeague.Webserver.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FoosballProLeague.Webserver.Controllers
{
    public class HomePageController : Controller
    {
        private readonly IHomePageLogic _homePageLogic;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomePageController(IHomePageLogic homePageLogic, IHttpContextAccessor httpContextAccessor)
        {
            _homePageLogic = homePageLogic;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("HomePage")]
        public async Task<IActionResult> HomePage()
        {
            return await GetUsersAndMatchHistory("1v1");
        }

        [HttpGet("HomePage/1v1")]
        public async Task<IActionResult> GetUsers1v1()
        {
            return await GetUsersAndMatchHistory("1v1");
        }

        [HttpGet("HomePage/2v2")]
        public async Task<IActionResult> GetUsers2v2()
        {
            return await GetUsersAndMatchHistory("2v2");
        }


        [HttpGet]
        public async Task<IActionResult> GetUsersAndMatchHistory(string mode)
        {
            try
            {
                List<UserModel> users = await _homePageLogic.GetUsers();
                UserModel user = GetUserFromCookie();
                List<MatchHistoryViewModel> matchHistory = null;

                if (user != null)
                {
                    try
                    {
                        List<MatchHistoryModel> matchHistoryModels = await _homePageLogic.GetMatchHistoryByUserId(user.Id);
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
                            TimeAgo = _homePageLogic.GetTimeAgo(m.EndTime)
                        }).ToList();
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, "Could not load match history");
                    }
                }
                
                HomePageViewModel viewModel = new HomePageViewModel
                {
                    Users = users,
                    MatchHistory = matchHistory,
                    Mode = mode
                };
                return View("HomePage", viewModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Could not load leaderboard");
                return View("HomePage", new HomePageViewModel { Users = new List<UserModel>(), MatchHistory = null, Mode = mode });
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
    }
}
