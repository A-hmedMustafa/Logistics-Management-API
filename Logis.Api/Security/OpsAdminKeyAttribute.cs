using Logis.Api.Security.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Logis.Api.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class OpsAdminKeyAttribute : Attribute, IAsyncActionFilter
    {
        private const string HeaderName = "X-Admin-Key";
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var options = context.HttpContext.RequestServices.GetRequiredService<IOptions<OpsAdminOptions>>().Value;
            if(!options.Enabled)
            {
                context.Result = new NotFoundResult();
                return;
            }

            if(!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var admin))
            {
                context.Result = new UnauthorizedObjectResult(new { error = "Missing X-Admin-Key Header." });
                return;
            }

            if(!string.Equals(admin.ToString(),options.AdminKey, StringComparison.Ordinal))
            {
                context.Result = new UnauthorizedObjectResult(new {error = "Invalid X-Admin-Key Header."});
                return;
            }

            await next();
        }
    }
}
