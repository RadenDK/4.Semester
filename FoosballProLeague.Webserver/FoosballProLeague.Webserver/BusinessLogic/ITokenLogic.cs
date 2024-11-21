using System.IdentityModel.Tokens.Jwt;

namespace FoosballProLeague.Webserver.BusinessLogic
{
    public interface ITokenLogic
    {
        Task<string> ValidateAndGetNewJwt(string jwt);
        public JwtSecurityToken GetJWTFromCookie();
    }
}
