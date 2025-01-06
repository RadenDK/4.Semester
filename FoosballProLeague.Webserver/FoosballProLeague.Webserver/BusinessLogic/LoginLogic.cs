using FoosballProLeague.Webserver.BusinessLogic.Interfaces;
using FoosballProLeague.Webserver.Models;
using FoosballProLeague.Webserver.Service.Interfaces;
using System.Net.Http;


namespace FoosballProLeague.Webserver.BusinessLogic;

public class LoginLogic : ILoginLogic
{
    
    private readonly ILoginService _loginService;
    
    
    public LoginLogic(ILoginService loginService)
    {
        _loginService = loginService;
    }
    
    //Logic for login user
    public async Task<HttpResponseMessage> LoginUser(string email, string password)
    {
        return await _loginService.LoginUser(email, password);
    }
    
}