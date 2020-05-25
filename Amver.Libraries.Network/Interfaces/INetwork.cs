using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Amver.Domain.Models;

namespace Amver.Libraries.Network.Interfaces
{
    public interface INetwork
    {
        Task<(BaseResult baseResult, string response)> LoadDataPostAsync(string url, string serializedObj, string bearerToken);
        
        Task<(BaseResult baseResult, string response)> LoadDataGetAsync(string url, string bearerToken);

        Task<(BaseResult baseResult, string response)> LoadFilePostAsync(string url, byte[] file, string bearerToken);
    }
}