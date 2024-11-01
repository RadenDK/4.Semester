using System.Text.Json;
using FoosballProLeague.Webserver.Models;
using FoosballProLeague.Webserver.BusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

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
            
            string accessToken = await response.Content.ReadAsStringAsync();
            Response.Cookies.Append("accessToken", accessToken, new CookieOptions 
                { 
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                    
                     });
            
            return RedirectToAction("HomePage", "HomePage");
        }

        string errorContent = await response.Content.ReadAsStringAsync();
        string errorMessage = JsonDocument.Parse(errorContent).RootElement.GetProperty("message").GetString();
        ModelState.AddModelError(string.Empty, errorMessage);
        return View("Login");
    }

    public async Task<IActionResult> Logout()
    {
        return RedirectToAction("Login", "Login");
    }
}