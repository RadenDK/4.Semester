using Microsoft.AspNetCore.Mvc;
using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.Models;

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
                return BadRequest(ex.Message);
            }
        }
    }
}


