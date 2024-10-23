using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoosballProLeague.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class DepartmentController : Controller
{
    private ICompanyLogic _companyLogic;
    
    public DepartmentController(ICompanyLogic companyLogic)
    {
        _companyLogic = companyLogic;
    }
    
    [HttpGet]
    public IActionResult GetDepartments()
    {
        try
        {
            List<DepartmentModel> departments = _companyLogic.GetDepartments();
            return Ok(departments);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}