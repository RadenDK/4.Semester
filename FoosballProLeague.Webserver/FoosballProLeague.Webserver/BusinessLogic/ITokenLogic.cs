namespace FoosballProLeague.Webserver.BusinessLogic
{
    public interface ITokenLogic
    {
        Task<bool> ValidateJwt(string jwt);
    }
}
