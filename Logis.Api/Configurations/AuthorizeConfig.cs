using Logis.Application.Authz.Services;
using Logis.Infrastructure.Authz;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Logis.Api.Configurations
{
    public static class AuthorizeConfig
    {
        public static IServiceCollection AddApiAuthorization(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthorization();
            services.AddScoped<IOwnershipGaurdService, OwnershipGuardService>();
            return services;
        }
    }
}
