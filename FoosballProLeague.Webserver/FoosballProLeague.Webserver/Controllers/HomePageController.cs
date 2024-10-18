using FoosballProLeague.Webserver.BusinessLogic;
using FoosballProLeague.Webserver.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoosballProLeague.Webserver.Controllers
{
    public class HomePageController : Controller
    {
        private readonly IHomePageLogic _homePageLogic;

        public HomePageController(IHomePageLogic homePageLogic)
        {
            _homePageLogic = homePageLogic;
        }

        [HttpGet("HomePage")]
        public async Task<IActionResult> HomePage()
        {
            return await GetUsers();
            
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                List<UserModel> users = await _homePageLogic.GetUsers();
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
