using System.IdentityModel.Tokens.Jwt;

namespace FoosballProLeague.Webserver.BusinessLogic.Interfaces
{
    public interface ITokenLogic
    {
        Task<string> ValidateAndGetNewJwt(string jwt);
        public JwtSecurityToken GetJWTFromCookie();
    }
}
