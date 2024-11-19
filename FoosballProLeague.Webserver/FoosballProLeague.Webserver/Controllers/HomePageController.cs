using FoosballProLeague.Webserver.BusinessLogic;
using FoosballProLeague.Webserver.Models;
using Microsoft.AspNetCore.Authorization;
using FoosballProLeague.Webserver.Service;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FoosballProLeague.Webserver.Controllers
{
    [JwtAuthorize]
    public class HomePageController : Controller
    {
        private readonly IHomePageLogic _homePageLogic;
        

        public HomePageController(IHomePageLogic homePageLogic, IHttpContextAccessor httpContextAccessor)
        {
            _homePageLogic = homePageLogic;
        }

        
        [HttpGet("HomePage")]
        public async Task<IActionResult> HomePage()
        {
            try
            {
                HomePageViewModel viewModel = await _homePageLogic.GetUsersAndMatchHistory("1v1");
                return View("HomePage", viewModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Could not load leaderboard or match history");
                return View("HomePage", new HomePageViewModel { Users = new List<UserModel>(), MatchHistory = null });
            }
        }

        [HttpGet("HomePage/1v1")]
        public async Task<IActionResult> GetUsers1v1()
        {
            try
            {
                HomePageViewModel viewModel = await _homePageLogic.GetUsersAndMatchHistory("1v1");
                return View("HomePage", viewModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Could not load leaderboard or match history");
                return View("HomePage", new HomePageViewModel { Users = new List<UserModel>(), MatchHistory = null });
            }
        }
        
        [HttpGet("HomePage/2v2")]
        public async Task<IActionResult> GetUsers2v2()
        {
            try
            {
                HomePageViewModel viewModel = await _homePageLogic.GetUsersAndMatchHistory("2v2");
                return View("HomePage", viewModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Could not load leaderboard or match history");
                return View("HomePage", new HomePageViewModel { Users = new List<UserModel>(), MatchHistory = null });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(string mode, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                List<UserModel> users = await _homePageLogic.GetLeaderboards(mode, pageNumber, pageSize);
                int totalUserCount = await _homePageLogic.GetTotalUserCount(mode);

                HomePageViewModel viewModel = new HomePageViewModel
                {
                    Users = users,
                    Mode = mode,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalUserCount = totalUserCount
                };
                
                return View("HomePage", viewModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Could not load leaderboard or match history");
                return View("HomePage", new HomePageViewModel { Users = new List<UserModel>(), MatchHistory = null });
            }
        }
        
        [HttpGet("api/User")]
        public async Task<IActionResult> GetUsersJson(string mode, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                List<UserModel> users = await _homePageLogic.GetLeaderboards(mode, pageNumber, pageSize);
                int totalUserCount = await _homePageLogic.GetTotalUserCount(mode);
                return Json(new { users, totalUserCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
