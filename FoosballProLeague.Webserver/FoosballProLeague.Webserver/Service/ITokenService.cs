namespace FoosballProLeague.Webserver.Service;

public interface ITokenService
{
    Task<HttpResponseMessage> ValidateJwt(string jwt);

}