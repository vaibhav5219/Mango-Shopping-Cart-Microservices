using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Mango.Services.AuthAPI.Service
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtOptions _jwtOptions;

        public JwtTokenGenerator(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
        }
        public string GenerateToken(ApplicationUser applicationUser)
        {
            try
            {
                // 1st create token key handler
                var tokenHandler = new JwtSecurityTokenHandler();

                // 2nd create secret key object from appsettings
                var key = Encoding.ASCII.GetBytes(_jwtOptions.Secret);

                // 3rd Create claims list ( cliam is a Key value pair)
                var cliamList = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Name, applicationUser.Name),
                new Claim(JwtRegisteredClaimNames.Email, applicationUser.Email),
                new Claim(JwtRegisteredClaimNames.Sub, applicationUser.Id),
            };

                // 4th Create token secriptor
                var tokenDescriptor = new SecurityTokenDescriptor()
                {
                    Audience = _jwtOptions.Audience,
                    Issuer = _jwtOptions.Issuer,
                    Subject = new ClaimsIdentity(cliamList),    //  Consume claimList
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature) // consume secret key
                };

                // 5th create token
                var token = tokenHandler.CreateToken(tokenDescriptor);

                // 6th return token using writeToken
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
