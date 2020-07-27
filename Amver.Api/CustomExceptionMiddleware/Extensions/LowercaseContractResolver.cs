using Newtonsoft.Json.Serialization;

namespace Amver.Api.CustomExceptionMiddleware.Extensions
{
    public class LowercaseContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLower();
        }
    }
}