using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Logis.Api.Contratcs
{
    public class ApiValidationProblemFactory
    {
        public static IActionResult Create(HttpContext context, ModelStateDictionary modelState)
        {
            // Construct The Problem Object That Would Return
            var problem = new ValidationProblemDetails(modelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Failed",
                Type = " https://httpstatuses.com/400",
                Instance = context.Request.Path
            };

            problem.Extensions["traceId"] = context.TraceIdentifier;

            if (context.Items.TryGetValue("X-Correlation-Id", out var correlationId) && correlationId is not null)
                problem.Extensions["correlationId"] = correlationId;

            problem.Extensions["timestampUtc"] = DateTime.UtcNow;

            return new ObjectResult(problem)
            {
                ContentTypes = { "application/problem+json" },
                StatusCode = problem.Status
            };
        }
    }
}
