using Microsoft.AspNetCore.Builder;

namespace Amver.WebAPI.CustomExceptionMiddleware.Extensions
{
    internal static class ExceptionMiddlewareExtensions
    {
        public static void UseCustomExceptionMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionMiddleware>();
        }
    }
}