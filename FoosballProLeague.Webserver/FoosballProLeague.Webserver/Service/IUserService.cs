using FoosballProLeague.Webserver.Models;

namespace FoosballProLeague.Webserver.Service;

public interface IUserService
{
    Task<HttpResponseMessage> SendUserToApi(UserRegistrationModel newUser);
}