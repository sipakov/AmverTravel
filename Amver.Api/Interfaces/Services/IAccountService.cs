using System.Threading.Tasks;
using Amver.Domain.Models;

namespace Amver.Api.Interfaces.Services
{
    public interface IAccountService
    {
        Task<TokenResponse> CreateAnAccount(UserCredential userCredential);

        Task<TokenResponse> ClassicLogin(UserCredential userCredential);
        
        Task<TokenResponse> FbLogin(string marker);
    }
}