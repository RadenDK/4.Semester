using FoosballProLeague.Webserver.BusinessLogic;
using FoosballProLeague.Webserver.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FoosballProLeague.Webserver.Controllers
{
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
    }
}
