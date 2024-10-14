using FoosballProLeague.Webserver.Models;

namespace FoosballProLeague.Webserver.BusinessLogic;

public interface IUserLogic
{
    Task<HttpResponseMessage> SendUserToApi(UserRegistrationModel newUser);
}