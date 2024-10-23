using Microsoft.AspNetCore.Mvc;
using FoosballProLeague.Webserver.Models;
using FoosballProLeague.Webserver.BusinessLogic;

namespace FoosballProLeague.Webserver.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly IRegistrationLogic _registrationLogic;

        public RegistrationController(IRegistrationLogic registrationLogic)
        {
            _registrationLogic = registrationLogic;
        }
        
        [HttpGet("Registration")]
        public async Task<IActionResult> Registration()
        {
            await PopulateViewBags();
            return View("Registration", new UserRegistrationModel());
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(UserRegistrationModel newUser)
        {
            if(!ModelState.IsValid)
            {
                await PopulateViewBags();
                return View("Registration", newUser);
            }
            
            HttpResponseMessage response = await _registrationLogic.SendUserToApi(newUser);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "An error occurred while creating the user");
                await PopulateViewBags();
                return View("Registration", newUser);
            }
        }
        
        private async Task PopulateViewBags()
        {
            ViewBag.Companies = await _registrationLogic.GetCompaniesAsync();
            ViewBag.Departments = await _registrationLogic.GetDepartments();
        }
    }
}