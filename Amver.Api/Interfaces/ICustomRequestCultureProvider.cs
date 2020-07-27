
namespace Amver.Api.Interfaces
{
    public interface ICustomRequestCultureProvider
    {
        string DetermineProviderCultureResult(Microsoft.AspNetCore.Http.HttpContext httpContent);
    }
}