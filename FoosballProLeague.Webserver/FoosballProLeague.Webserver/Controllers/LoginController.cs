using System.Text.Json;
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

    [HttpGet("")]
    [HttpGet("Login")]
    public IActionResult Login()
    {
        return View();
    }
    
    
    // Login method
    [HttpPost("Login")]
    public async Task<IActionResult> LoginUser(LoginUserModel user)
    {
        HttpResponseMessage response = await _loginLogic.LoginUser(user.Email, user.Password);
        
        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("HomePage", "HomePage");
        }

        string errorContent = await response.Content.ReadAsStringAsync();
        string errorMessage = JsonDocument.Parse(errorContent).RootElement.GetProperty("message").GetString();
        ModelState.AddModelError(string.Empty, errorMessage);
        return View("Login");
    }
}