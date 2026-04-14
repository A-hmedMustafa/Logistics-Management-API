namespace Logis.Api.Middleware
{
    public sealed class CorrelationIdMiddleware : IMiddleware
    {
        public const string HeaderName = "X-Correlation-Id";
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // 1) Read The CorrelationId from The Request Header
            var correlatioId = context.Request.Headers[HeaderName];

            // 2) If No CorrelationId --> Make New One  
            if (string.IsNullOrWhiteSpace(correlatioId))
                correlatioId = Guid.NewGuid().ToString();

            // 3) Store It Here Allows Us To Read Anytime During The Request LifeCycle, Whitout Having To Read Headers
            context.Items[HeaderName] = correlatioId;

            // 4) OnStarting Assures Storing The CorrelationId in The Header, Even Something Wrong Happen
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[HeaderName] = correlatioId;
                return Task.CompletedTask;
            });

            // 5) Move To The Next Middleware 
            await next(context);
        }
    }
}
