using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Daimler.Providence.Service.Authorization
{
    public class SwaggerOAuthMiddleware
    {
        private readonly RequestDelegate next;
        public SwaggerOAuthMiddleware(RequestDelegate next)
        {
            this.next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            if (IsSwagger(context.Request.Path))
            {
                // if user is not authenticated
                if (!context.User.Identity.IsAuthenticated)
                {
                    await context.ChallengeAsync();
                    return;
                }
            }
            await next.Invoke(context);
        }
        public bool IsSwagger(PathString pathString)
        {
            return pathString.StartsWithSegments("/swagger");
        }
    }
}
