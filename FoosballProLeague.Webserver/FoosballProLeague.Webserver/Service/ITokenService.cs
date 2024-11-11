namespace FoosballProLeague.Webserver.Service;

public interface ITokenService
{
    Task<HttpResponseMessage> ValidateJwtAndGetNewJwt(string jwt);

}