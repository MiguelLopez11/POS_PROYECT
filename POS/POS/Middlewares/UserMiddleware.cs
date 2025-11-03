using Microsoft.EntityFrameworkCore;
using POS.Data;

namespace POS.Middlewares
{
    public class UserMiddleware
    {
        private readonly RequestDelegate _next;

        public UserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, POSDbContext dbContext)
        {
            if (context.Request.Headers.TryGetValue("User-Id", out var userIdHeader))
            {
                if (int.TryParse(userIdHeader, out int userId))
                {
                    var user = await dbContext.Users
                        .FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);

                    if (user != null)
                    {
                        context.Items["User"] = user;
                    }
                }
            }

            await _next(context);
        }
    }

    public static class UserMiddlewareExtensions
    {
        public static IApplicationBuilder UseUserMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserMiddleware>();
        }
    }
}

