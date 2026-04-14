using Logis.Api.Middleware;
using Logis.Api.Observability;
using Logis.Api.Security;
using Logis.Api.Security.Options;
using Logis.Application.Auth.Options;
using Logis.Application.Auth.Services.Auth;
using Logis.Application.Auth.Services.Security;
using Logis.Application.Auth.Services.Sessions;
using Logis.Application.Notifications.Services;
using Logis.Application.Ops.Outbox.Services;
using Logis.Application.Orders.Services;
using Logis.Application.Tracking.Services;
using Logis.Infrastructure.Auth.Sevices.Auth.AuthServices;
using Logis.Infrastructure.Auth.Sevices.Auth.JwtService;
using Logis.Infrastructure.Auth.Sevices.Auth.RefreshTokens;
using Logis.Infrastructure.Auth.Sevices.Sessions;
using Logis.Infrastructure.Notifications.Services;
using Logis.Infrastructure.Ops.Outbox;
using Logis.Infrastructure.Orders.Services;
using Logis.Infrastructure.Shipments.Services;
using Logis.Infrastructure.Tracking.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Logis.Api.Configurations
{
    public static class DIConfig
    {
        public static IServiceCollection AddApiDependences(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            services.Configure<RefreshTokenOptions>(configuration.GetSection("RefreshTokens"));
            services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

            // Ensure App startup fails if JWT config is wrong
            services.AddOptions<JwtOptions>()
                .Bind(configuration.GetSection("Jwt"))
                .Validate(options=> !string.IsNullOrWhiteSpace(options.Issuer) 
                && !string.IsNullOrWhiteSpace(options.Audience) && options.SigningKeys.Any(),"Invalid Jwt Configuration");

            services.AddSingleton<JwtTokenService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<ICurrentUser, CurrentUser>();
            services.AddScoped<ISessionService, SessionService>();
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IRefresherService, RefesherService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IShipmentService, ShipmentService>();
            services.AddTransient<ApiExceptionMiddleware>();
            services.AddTransient<CorrelationIdMiddleware>();
            services.AddTransient<RequestLoggingMiddleware>();
            services.AddSingleton<IDomainEventLogger, DomainEventLogger>();
            services.AddScoped<IShipmentTrackingService, ShipmentTrackingService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IOutBoxEnqueuer,OutBoxEnqueuer>();
            services.AddHostedService<OutboxProcessorHostedService>();
            services.AddScoped<IOutboxAdminQueryService, OutboxAdminQueryService>();
            services.AddScoped<IOutboxAdminRecoveryService, OutboxAdminRecoveryService>();
            services.Configure<OpsAdminOptions>(configuration.GetSection(OpsAdminOptions.SectionName));
            return services;
        }
    }
}
