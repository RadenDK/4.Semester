using FoosballProLeague.Webserver.Models;

namespace FoosballProLeague.Webserver.Service;

public interface ILoginService
{
    Task<HttpResponseMessage> LoginUser(LoginUserModel loginModel);
}