using System.Text.Json;
using FoosballProLeague.Webserver.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using FoosballProLeague.Webserver.BusinessLogic.Interfaces;

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
        
        return View("Login");
    }

    public async Task<IActionResult> Logout()
    {
        //Remove the access token from the client
        Response.Cookies.Delete("accessToken");
        
        //Redirect to the login page
        return RedirectToAction("Login", "Login");
    }
}