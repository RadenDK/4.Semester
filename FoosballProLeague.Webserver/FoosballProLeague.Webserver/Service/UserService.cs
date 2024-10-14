using FoosballProLeague.Webserver.Models;

namespace FoosballProLeague.Webserver.Service;

public class UserService : IUserService
{
    public async Task<HttpResponseMessage> SendUserToApi(UserRegistrationModel newUser)
    {
        return new HttpResponseMessage();
    }
}