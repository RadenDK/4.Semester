using FoosballProLeague.Api.BusinessLogic;
using FoosballProLeague.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoosballProLeague.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class DepartmentController : Controller
{
    private IDepartmentLogic _departmentLogic;
    
    public DepartmentController(IDepartmentLogic departmentLogic)
    {
        _departmentLogic = departmentLogic;
    }
    
    [HttpGet]
    public IActionResult GetDepartments()
    {
        try
        {
            List<DepartmentModel> departments = _departmentLogic.GetDepartments();
            return Ok(departments);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}