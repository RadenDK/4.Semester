namespace FoosballProLeague.Api.BusinessLogic
{
    public interface ITokenLogic
    {
        string GenerateJWT();
        bool ValidateJWT(string jwt);
    }
}
