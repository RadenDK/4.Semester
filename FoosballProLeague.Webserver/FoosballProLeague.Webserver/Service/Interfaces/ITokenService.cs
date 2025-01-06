namespace FoosballProLeague.Webserver.Service.Interfaces;

public interface ITokenService
{
    Task<HttpResponseMessage> ValidateJwtAndGetNewJwt(string jwt);

}