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
            return await GetCompanies();
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            try
            {
                List<CompanyModel> companies = await _registrationLogic.GetCompaniesAsync();
                ViewBag.Companies = companies;
                return View("Registration", new UserRegistrationModel());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(UserRegistrationModel newUser)
        {
            if(!ModelState.IsValid)
            {
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
                return View("Registration", newUser);
            }
        }

        [HttpPost("Departments")]
        public async Task<IActionResult> GetDepartments([FromBody] int companyId)
        {
            try
            {
                List<DepartmentModel> departments = await _registrationLogic.GetDepartmentByCompanyId(companyId);
                List<CompanyModel> companies = await _registrationLogic.GetCompaniesAsync();

                ViewBag.Companies = companies;
                ViewBag.Departments = departments;
                
                UserRegistrationModel userRegistrationModel = new UserRegistrationModel { CompanyId = companyId };
                return View("Registration", userRegistrationModel);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}