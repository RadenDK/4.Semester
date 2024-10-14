using FoosballProLeague.Webserver.Models;
using FoosballProLeague.Webserver.Service;

namespace FoosballProLeague.Webserver.BusinessLogic;

public class UserLogic : IUserLogic
{
    private readonly IUserService _userService;
    
    public UserLogic(IUserService userService)
    {
        _userService = userService;
    }
    
    public async Task<HttpResponseMessage> SendUserToApi(UserRegistrationModel newUser)
    {
        return await _userService.SendUserToApi(newUser);
    }
}