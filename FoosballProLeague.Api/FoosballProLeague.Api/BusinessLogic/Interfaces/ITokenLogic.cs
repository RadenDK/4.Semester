using FoosballProLeague.Api.Models;

namespace FoosballProLeague.Api.BusinessLogic.Interfaces
{
    public interface ITokenLogic
    {
        string GenerateJWT(UserModel user);
        bool ValidateJWT(string jwt);
        int GetUserIdFromJWT(string authorizationHeader);
    }
}
