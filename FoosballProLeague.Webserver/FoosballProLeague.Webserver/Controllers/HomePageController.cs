using FoosballProLeague.Webserver.BusinessLogic;
using FoosballProLeague.Webserver.Models;
using FoosballProLeague.Webserver.Service;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoosballProLeague.Webserver.Controllers
{
    public class HomePageController : Controller
    {
        private readonly IHomePageLogic _homePageLogic;
        private readonly LeaderboardService _leaderboardService;

        public HomePageController(IHomePageLogic homePageLogic, LeaderboardService leaderboardService)
        {
            _homePageLogic = homePageLogic;
            _leaderboardService = leaderboardService;
        }

        [HttpGet("HomePage")]
        public async Task<IActionResult> HomePage(int pageNumber = 1, int pageSize = 10)
        {
            return await GetUsers("1v1", pageNumber, pageSize);
        }

        [HttpGet("HomePage/1v1")]
        public async Task<IActionResult> GetUsers1v1(int pageNumber = 1, int pageSize = 10)
        {
            return await GetUsers("1v1", pageNumber, pageSize);
        }

        [HttpGet("HomePage/2v2")]
        public async Task<IActionResult> GetUsers2v2(int pageNumber = 1, int pageSize = 10)
        {
            return await GetUsers("2v2", pageNumber, pageSize);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(string mode, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                List<UserModel> users = await _leaderboardService.GetSortedLeaderboard(mode);
                int totalUserCount = users.Count;
                ViewBag.Mode = mode;
                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalUserCount = totalUserCount;
                return View("HomePage", users);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Could not load leaderboard");
                return View("HomePage", new List<UserModel>());
            }
        }
    }
}