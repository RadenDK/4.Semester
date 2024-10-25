using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using FoosballProLeague.Webserver.Models;
using FoosballProLeague.Webserver.BusinessLogic;
using Newtonsoft.Json.Linq;

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
                TempData["SuccesMessage"] = "Account was created succesfully!";
                return RedirectToAction("Login", "Login");
            }
            else
            {
                HandleErrorResponse(response);
                await PopulateViewBags();
                return View("Registration", newUser);
            }
        }

        private async void HandleErrorResponse(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var errorMessage = await GetErrorMessageFromResponse(response);
                if (errorMessage == ApiErrorMessages.EmailExistsCode)
                {
                    ModelState.AddModelError("Email", "Email already exists");
                }
            }
            else
            {
                TempData["ErrorMessage"] = "An error occurred while trying to create the account. Please try again later.";
            }
            
        }
        
        private async Task<string> GetErrorMessageFromResponse(HttpResponseMessage response)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var errorResponse = JObject.Parse(responseContent);
            return errorResponse["message"].ToString();
        }
        
        private async Task PopulateViewBags()
        {
            ViewBag.Companies = await _registrationLogic.GetCompaniesAsync();
            ViewBag.Departments = await _registrationLogic.GetDepartments();
        }
    }
}