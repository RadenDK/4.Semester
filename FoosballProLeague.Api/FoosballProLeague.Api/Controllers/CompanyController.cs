using Microsoft.AspNetCore.Mvc;
using FoosballProLeague.Api.Models;
using FoosballProLeague.Api.BusinessLogic.Interfaces;

namespace FoosballProLeague.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyController : Controller
    {
        private ICompanyLogic _companyLogic;
        
        public CompanyController(ICompanyLogic companyLogic)
        {
            _companyLogic = companyLogic;
        }
        
        [HttpGet]
        public IActionResult GetCompanies()
        {
            try
            {
                List<CompanyModel> companies = _companyLogic.GetCompanies();
                return Ok(companies);
            }
            catch (Exception ex)
            {
                return BadRequest("Something went wrong during retrieval");
            }
        }
    }
}


