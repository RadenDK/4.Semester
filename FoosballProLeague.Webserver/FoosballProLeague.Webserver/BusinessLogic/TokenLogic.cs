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

        public async Task<bool> ValidateJwt(string jwt)
        {
            bool success = false;

            HttpResponseMessage response = await _tokenService.ValidateJwt(jwt);

            if (response.IsSuccessStatusCode)
            {
                success = true;
            }

            return success;
        }
    }
}
