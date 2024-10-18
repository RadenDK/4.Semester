using FoosballProLeague.Webserver.Models;
using FoosballProLeague.Webserver.Service;

namespace FoosballProLeague.Webserver.BusinessLogic;

public class RegistrationLogic : IRegistrationLogic
{
    private readonly IRegistrationService _registrationService;
    
    public RegistrationLogic(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }
    
    public async Task<HttpResponseMessage> SendUserToApi(UserRegistrationModel newUser)
    {
        return await _registrationService.SendUserToApi(newUser);
    }
}