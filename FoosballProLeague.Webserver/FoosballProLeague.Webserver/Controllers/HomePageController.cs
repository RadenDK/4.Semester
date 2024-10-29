using FoosballProLeague.Webserver.BusinessLogic;
using FoosballProLeague.Webserver.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace FoosballProLeague.Webserver.Controllers
{
    public class HomePageController : Controller
    {
        private readonly IHomePageLogic _homePageLogic;
        private readonly HttpClient _httpClient; // er httpclient nødvendig her? virker overflødig hvis den er korrekt implementeret i HomePageService, no??

        public HomePageController(IHomePageLogic homePageLogic, IHttpClientFactory httpClientFactory)
        {
            _homePageLogic = homePageLogic;
            _httpClient = httpClientFactory.CreateClient("APIClient");
        }

        [HttpGet("HomePage")]
        public async Task<IActionResult> HomePage()
        {
            return await GetUsers("1v1");
        }

        [HttpGet("HomePage/1v1")]
        public async Task<IActionResult> GetUsers1v1()
        {
            return await GetUsers("1v1");
        }

        [HttpGet("HomePage/2v2")]
        public async Task<IActionResult> GetUsers2v2()
        {
            return await GetUsers("2v2");
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(string mode)
        {
            try
            {
                List<UserModel> users = await _homePageLogic.GetUsers();
                ViewBag.Mode = mode;
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