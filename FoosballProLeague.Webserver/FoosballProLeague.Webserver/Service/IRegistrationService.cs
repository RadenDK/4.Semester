using FoosballProLeague.Webserver.Models;

namespace FoosballProLeague.Webserver.Service;

public interface IRegistrationService
{
    Task<HttpResponseMessage> SendUserToApi(UserRegistrationModel newUser);
}