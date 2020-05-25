using System;
using System.Linq;
using Amver.WebApi.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Amver.WebApi.Implementations
{
    public class CustomRequestCultureProvider : ICustomRequestCultureProvider
    {
        public string DetermineProviderCultureResult(Microsoft.AspNetCore.Http.HttpContext httpContent)
        {
            if (httpContent == null) throw new ArgumentNullException(nameof(httpContent));

            var requestFeature = httpContent.Request.GetTypedHeaders().AcceptLanguage;

            var cultureStr = requestFeature?.First().Value;

            return cultureStr?.Value;
        }
    }
}