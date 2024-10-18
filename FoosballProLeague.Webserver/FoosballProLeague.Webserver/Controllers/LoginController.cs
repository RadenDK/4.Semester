using FoosballProLeague.Webserver.Models;
using FoosballProLeague.Webserver.BusinessLogic;
using Microsoft.AspNetCore.Mvc;

namespace FoosballProLeague.Webserver.Controllers;

public class LoginController : Controller
{
    private readonly ILoginLogic _loginLogic;
    
    public LoginController(ILoginLogic loginLogic)
    {
        _loginLogic = loginLogic;
    }
    
    [HttpGet("Login")]
    public IActionResult Login()
    {
        return View();
    }
    
    
    // Login method
    [HttpPut("Login")]
    public async Task<IActionResult> LoginUser(UserModel user)
    {
        return View();
    }
    
    
    // Logout method
    [HttpPut("Logout")]
    public async Task<IActionResult> LogoutUser(UserModel user)
    {
        return View();
    }
}