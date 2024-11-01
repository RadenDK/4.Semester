using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FoosballProLeague.Api.BusinessLogic
{
    public class TokenLogic : ITokenLogic
    {
        private readonly IConfiguration _configuration;



        public TokenLogic(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateJWT()
        {
            // Get the secret key from configuration
            string signingKey = _configuration["Jwt:signingKey"];
            if (string.IsNullOrEmpty(signingKey))
            {
                throw new InvalidOperationException("JWT signing key is not configured");
            }

            // Define token handler 
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            // Have a list of claims currently empty but could later be filled with user data
            List<Claim> claims = new List<Claim>
            {
            };

            // Define token descriptor with key and optional claims
            DateTime expireTime = DateTime.UtcNow.AddHours(1); // have the token expire after an hour

            Byte[] endcodedSigningKey = Encoding.UTF8.GetBytes(signingKey);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expireTime, 
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(endcodedSigningKey), SecurityAlgorithms.HmacSha256Signature)
            };

            // Create token
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            // Return serialized token string
            string serializedToken = tokenHandler.WriteToken(token);
            return serializedToken;
        }

        public bool ValidateJWT(string jwt)
        {
            throw new NotImplementedException();
        }
    }
}
