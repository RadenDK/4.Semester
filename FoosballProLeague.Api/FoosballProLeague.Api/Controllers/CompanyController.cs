using Microsoft.AspNetCore.Mvc;
using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.Models;

namespace FoosballProLeague.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CompanyController : Controller
    {
        private ICompanyLogic _companyLogic;
        
        public CompanyController(ICompanyLogic companyLogic)
        {
            _companyLogic = companyLogic;
        }
        
        [HttpGet("companies")]
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

        [HttpPost("departments")]
        public IActionResult GetDepartments([FromBody] int companyId)
        {
            try
            {
                List<DepartmentModel> departments = _companyLogic.GetDepartments(companyId);
                return Ok(departments);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}


