using Microsoft.AspNetCore.Builder;

namespace Amver.Api.CustomExceptionMiddleware.Extensions
{
    internal static class ExceptionMiddlewareExtensions
    {
        public static void UseCustomExceptionMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionMiddleware>();
        }
    }
}