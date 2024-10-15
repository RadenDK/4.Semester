using Microsoft.AspNetCore.Mvc;
using FoosballProLeague.Webserver.Models;
using FoosballProLeague.Webserver.BusinessLogic;

namespace FoosballProLeague.Webserver.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserLogic _userLogic;

        public UserController(IUserLogic userLogic)
        {
            _userLogic = userLogic;
        }

        [HttpGet("Registration")]
        public IActionResult Registration()
        {
            return View();
        }
        

        [HttpPost]
        public async Task<IActionResult> CreateUser(UserRegistrationModel newUser)
        {
            if(!TryValidateModel(newUser))
            {
                return View("Registration", newUser);
            }

            HttpResponseMessage response = await _userLogic.SendUserToApi(newUser);
            return null;
        }
    }
}