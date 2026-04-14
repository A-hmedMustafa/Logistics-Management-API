
using Microsoft.Extensions.Primitives;
using System.Diagnostics;
using System.IO;
using System.Security.Claims;

namespace Logis.Api.Middleware
{
    public sealed class RequestLoggingMiddleware : IMiddleware
    {
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger)
        {
            _logger = logger;
        }

        // The Simple Goal Of This Middleware, Is To Get Some Info From The Request, And Response And Log Them In Console
        // Easy And Simple
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var stopWatch = Stopwatch.StartNew();
            var userId = TryGetUserId(context.User);
            var CorrelationId = TryGetcorrelationId(context);

            using(_logger.BeginScope(new Dictionary<string, object?>
            {
                ["correlationId"] = CorrelationId,
                ["userId"] = userId,
                ["path"] = context.Request.Path.ToString(),
            }))
          
            try
            {
                await next(context);
            }
            finally
            {
                stopWatch.Stop();

                var method = context.Request.Method;
                var path = context.Request.Path;
                var statusCode = context.Response.StatusCode;
                var elapsedTime = stopWatch.ElapsedMilliseconds;
                
                    // goal log --> HTTP POST /api/orders -> 201 in 84ms userId=... correlationId=...
                    // - 5xx => Error
                    // - 4xx => Warning
                    // - else => Information

                    if (statusCode >= 500)
                {
                    _logger.LogError(
                        "HTTP {Method} {Path} -> {StatusCode} in {ElapsedTime}ms",
                        method,path,statusCode,elapsedTime);
                }
                else if(statusCode >=400)
                {
                    _logger.LogWarning(
                        "HTTP {Method} {Path} -> {StatusCode} in {ElapsedTime}ms ",
                        method, path, statusCode, elapsedTime);
                }
                else
                {
                    _logger.LogInformation(
                        "HTTP {Method} {Path} -> {StatusCode} in {ElapsedTime}ms ",
                        method, path, statusCode, elapsedTime);
                }
            }
        }

        private static string? TryGetUserId(ClaimsPrincipal user)
        {
            // Checks First If The User Is Signed In -- (Has A JWT)
            if(user?.Identity?.IsAuthenticated != true) 
                return null;
            // First Try To Get The Id From NameIdentifier, If Not There Get It From sub
            return user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        }

        private static string? TryGetcorrelationId(HttpContext httpContext)
        {
            // 1) First Serach In Context.Items --> (We Stored It In CorrelationMiddleware, So It Would Be There Within The Same Request)
            if(httpContext.Items.TryGetValue(CorrelationIdMiddleware.HeaderName, out var correlationIdFromItems))
                return correlationIdFromItems?.ToString();

            // 2) Not There, Then Search In Response Header
            if(httpContext.Response.Headers.TryGetValue(CorrelationIdMiddleware.HeaderName,out StringValues correlationIdFromResponse))
                return correlationIdFromResponse.FirstOrDefault();

            if(httpContext.Request.Headers.TryGetValue(CorrelationIdMiddleware.HeaderName,out StringValues correlationIdFromRequest))
                return correlationIdFromRequest.FirstOrDefault();

            return null;
        }
    }
}
