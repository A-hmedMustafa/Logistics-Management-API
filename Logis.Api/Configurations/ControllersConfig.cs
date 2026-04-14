using Logis.Api.Contratcs;
using Logis.Api.Health;
using Logis.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Logis.Api.Configurations
{
    public static class ControllersConfig
    {
        public static IServiceCollection AddApiControllers(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOpenApi();
            services.AddControllers();
            services.AddDbContext<AppDbContext>(options =>
            {
                var connection = configuration.GetConnectionString("Default");
                options.UseSqlServer(connection);
                
                
            });

            services.AddHealthChecks()
                .AddDbContextCheck<AppDbContext>(name: "db", failureStatus: HealthStatus.Unhealthy, tags: new[] {"ready"})
                .AddCheck<DependencyHealthCheck>(name: "depends", failureStatus: HealthStatus.Unhealthy, tags: new[] {"ready"});

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context => 
                ApiValidationProblemFactory.Create(context.HttpContext, context.ModelState);
            });
            
            return services;
        }
    }
}
