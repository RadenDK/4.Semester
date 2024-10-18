using FoosballProLeague.Webserver.Models;

namespace FoosballProLeague.Webserver.BusinessLogic;

public interface IRegistrationLogic
{
    Task<HttpResponseMessage> SendUserToApi(UserRegistrationModel newUser);
}