using Logis.Application.Auth.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Logis.Api.Configurations
{
    public static class AuthenticationConfig
    {
        public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var JwtSection = configuration.GetSection("Jwt").Get<JwtOptions>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(Options =>
            {


                Options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = JwtSection.Issuer,

                    ValidateAudience = true,
                    ValidAudience = JwtSection.Audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
                    {
                        var keys = JwtSection.SigningKeys
                        .Where(k => k.Kid == kid)
                        .Select(k => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(k.Key)))
                        .ToList();
                        return keys;
                    },

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });
            return services;
        }
    }
}
