using Logis.Application.Auth.Options;
using Logis.Infrastructure.Auth.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Logis.Infrastructure.Auth.Sevices.Auth.JwtService
{
    public sealed class JwtTokenService
    {
        
        private readonly IConfiguration configuration;
        public JwtTokenService(IConfiguration configuration)
        {         
            this.configuration = configuration;
        }
        public string CreateAccessToken(AppUser user)
        {
            var JwtSection = configuration.GetSection("Jwt").Get<JwtOptions>();
            var activeKey = JwtSection.SigningKeys.First();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(activeKey.Key));
            
            if (key is null)
                throw new InvalidOperationException("Missing Secret Key");
            var signingCredintials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var userClaims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, user.Id.ToString()),
                new (ClaimTypes.Name, user.UserName ?? "")
            };

            if (!string.IsNullOrWhiteSpace(user.Email))
                userClaims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));

            var jwtToken = new JwtSecurityToken
                (issuer: JwtSection.Issuer,
                audience: JwtSection.Audience,
                claims: userClaims,
                expires: DateTime.UtcNow.AddMinutes(JwtSection.AccessTokenMinutes),
                signingCredentials: signingCredintials
                );

            jwtToken.Header["kid"] = activeKey.Kid;

            var stringToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            return stringToken;
        }
    }
}
