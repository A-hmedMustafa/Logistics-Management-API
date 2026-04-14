using Logis.Application.Auth.Options;
using Logis.Application.Auth.Services.Auth;
using Logis.Application.Auth.Services.Security;
using Logis.Application.Auth.Services.Sessions;
using Logis.Application.Authz.Services;
using Logis.Application.Orders.Services;
using Logis.Infrastructure.Auth.Identity;
using Logis.Infrastructure.Shipments.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Logis.Api.Health
{
    public sealed class DependencyHealthCheck : IHealthCheck
    {
        private readonly IServiceProvider _serviceProvider;

        public DependencyHealthCheck(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _serviceProvider.GetRequiredService<IOrderService>();
                _serviceProvider.GetRequiredService<IShipmentService>();
                _serviceProvider.GetRequiredService<ISessionService>();
                _serviceProvider.GetRequiredService<ICurrentUser>();
                _serviceProvider.GetRequiredService<ILoginService>();
                _serviceProvider.GetRequiredService<IRefresherService>();
                _serviceProvider.GetRequiredService<UserManager<AppUser>>();
                _serviceProvider.GetRequiredService<SignInManager<AppUser>>();
                _serviceProvider.GetRequiredService<IOwnershipGaurdService>();
                _serviceProvider.GetRequiredService<IOptions<JwtOptions>>();
                _serviceProvider.GetRequiredService<IOptions<RefreshTokenOptions>>();
                return Task.FromResult(HealthCheckResult.Healthy("Dependencies Resolved"));

            }catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Dependency Resolution Failed",ex));
            }
        }

    }

    public sealed class HealthResponseWriter
    {
        public static Task WriteJsonResponse(HttpContext context, HealthReport healthReport)
        {
            context.Response.ContentType = "application/json";

            var payload = new
            {
                status = healthReport.Status.ToString(),
                totalDurationInMs = healthReport.TotalDuration.TotalMilliseconds,
                checks = healthReport.Entries.Select(ent => new
                {
                    name=ent.Key,
                    status = ent.Value.Status.ToString(),
                    descreption = ent.Value.Description,
                    duration = ent.Value.Duration.TotalMilliseconds,
                    error = ent.Value.Exception?.Message
                })
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            }));
        }
    }
}
