using FoosballProLeague.Webserver.Service;

namespace FoosballProLeague.Webserver.BusinessLogic
{
    public class TokenLogic : ITokenLogic
    {

        private ITokenService _tokenService;

        public TokenLogic(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public async Task<string> ValidateAndGetNewJwt(string jwt)
        {
            HttpResponseMessage response = await _tokenService.ValidateJwt(jwt);

            string newAccessToken = await response.Content.ReadAsStringAsync();

            return newAccessToken;
        }
    }
}
