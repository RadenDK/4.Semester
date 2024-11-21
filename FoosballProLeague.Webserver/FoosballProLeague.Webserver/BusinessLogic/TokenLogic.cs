using FoosballProLeague.Webserver.Service;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Web;

namespace FoosballProLeague.Webserver.BusinessLogic
{
    public class TokenLogic : ITokenLogic
    {

        private ITokenService _tokenService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public TokenLogic(ITokenService tokenService, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _tokenService = tokenService;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public async Task<string> ValidateAndGetNewJwt(string jwt)
        {
            HttpResponseMessage response = await _tokenService.ValidateJwtAndGetNewJwt(jwt);

            string newAccessToken = await response.Content.ReadAsStringAsync();

            return newAccessToken;
        }

        public JwtSecurityToken GetJWTFromCookie()
        {
            string cookieValue = _httpContextAccessor.HttpContext.Request.Cookies["accessToken"];
            if (string.IsNullOrEmpty(cookieValue))
            {
                return null;
            }

            string decodedToken = HttpUtility.UrlDecode(cookieValue).Trim('"');

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(decodedToken))
            {
                throw new SecurityTokenMalformedException("The token is not in a valid format.");
            }

            JwtSecurityToken jsonToken = handler.ReadToken(decodedToken) as JwtSecurityToken;

            return jsonToken;
        }
    }
}
