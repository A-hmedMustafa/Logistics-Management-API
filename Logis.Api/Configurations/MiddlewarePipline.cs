using Logis.Api.Health;
using Logis.Api.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Logis.Api.Configurations
{
    public static class MiddlewarePipline
    {
        public static WebApplication UseApiPipeline(this WebApplication app)
        {
            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseMiddleware<ApiExceptionMiddleware>();


            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false,
                ResponseWriter =  HealthResponseWriter.WriteJsonResponse
            });

            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = checks => checks.Tags.Contains("ready"),
                ResponseWriter = HealthResponseWriter.WriteJsonResponse
            });

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            return app;
        }
    }
}
    