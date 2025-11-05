using Microsoft.AspNetCore.RateLimiting;

namespace POS.Middlewares
{
    public class RequestValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestValidationMiddleware> _logger;

        public RequestValidationMiddleware(RequestDelegate next, ILogger<RequestValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                var rateLimitMetadata = endpoint.Metadata.GetMetadata<EnableRateLimitingAttribute>();
                if (rateLimitMetadata != null)
                {
                    _logger.LogInformation($"Rate limiting applied to: {context.Request.Path}");
                }
            }

            await _next(context);
        }
    }

    public static class RateLimitDebugMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimitDebug(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestValidationMiddleware>();
        }
    }
}
