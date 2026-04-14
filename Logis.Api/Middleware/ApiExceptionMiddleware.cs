
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Logis.Api.Middleware
{
    public sealed class ApiExceptionMiddleware : IMiddleware
    {
        private readonly ILogger<ApiExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ApiExceptionMiddleware(ILogger<ApiExceptionMiddleware> logger, IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                var (statusCode,title) = MapException(ex);

                var traceId = context.TraceIdentifier;

                // - 4xx: expected client/domain issues (warning)
                // - 5xx: unexpected bug/infra issue (error + stack trace)

                if (statusCode >= 500)
                    _logger.LogError(ex, "Unhandled Exception. TraceId={TraceId} Path={Path}",
                        traceId, context.Request.Path);

                else
                    _logger.LogWarning(ex, "Request Error. StatusCode={StatusCode} TraceId={TraceId} Path={Path}",
                        statusCode,traceId,context.Request.Path);

                // Building The Response
                var problem = new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Instance = context.Request.Path,
                    Detail = _env.IsDevelopment() ? ex.Message : "An Error Occured While Processing Your Request."
                };
                problem.Type = statusCode >= 500 ? "https://httpstatuses.com/500" : $"https://httpstatuses.com/{statusCode}";
                problem.Extensions["traceId"] = traceId;

                context.Response.Clear();
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/problem+json";

                var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await context.Response.WriteAsync(json);
            }

        }

        private static (int StatusCode, string Title) MapException(Exception exception)
        {
            return exception switch
            {
                // Input / validation style errors (client mistakes)
                ArgumentOutOfRangeException => (StatusCodes.Status400BadRequest, "Invalid Request."),
                ArgumentException => (StatusCodes.Status400BadRequest, "Invalid Request."),
                ArithmeticException => (StatusCodes.Status400BadRequest, "Invalid Request."),

                // Not Found
                KeyNotFoundException => (StatusCodes.Status404NotFound,"Resource Not Found"),

                // Domain Rule/ State Transition Violation
                InvalidOperationException => (StatusCodes.Status409Conflict,"Operation Not Allowed In The Current State"),

                // Authorization Exception 
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "UnAuthorized"),

                // EF Core Related
                DbUpdateConcurrencyException => (StatusCodes.Status409Conflict,"Concurrency Conflict"),

                // Db Update Problems
                DbUpdateException  => (StatusCodes.Status409Conflict,"Database Constraint Conflict"),
                // Default Fallback
                _ => (StatusCodes.Status500InternalServerError,"UnExpected Server Error")

            };
        }
    }
}
