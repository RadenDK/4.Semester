namespace FoosballProLeague.Webserver.BusinessLogic
{
    public interface ITokenLogic
    {
        Task<string> ValidateAndGetNewJwt(string jwt);
    }
}
