using System;
using System.Threading.Tasks;
using Amver.WebAPI.CustomExceptionMiddleware.Extensions;
using Amver.WebAPI.CustomExceptionMiddleware.SimpleFactory;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Amver.WebAPI.CustomExceptionMiddleware
{
    internal class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex}");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var settings = new JsonSerializerSettings {ContractResolver = new LowercaseContractResolver()};

            return context.Response.WriteAsync(
                JsonConvert.SerializeObject(BaseResultFactory.CreateBaseResult(exception, context), settings));
        }               
    }
}