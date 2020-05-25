using System;
using System.Threading.Tasks;
using Amver.Domain.Entities;
using Amver.Domain.Models;

namespace Amver.WebApi.Interfaces.Storages
{
    public interface IAuthStorage
    {
        Task<UserAuthentication> CreateUserAuthenticationAsync(UserAuthentication userAuthentication, string fcmToken, int os);
        
        Task<bool> IsValidPasswordAsync(int userId, string hashedPassword);
        
        Task<UserAuth> GetActiveUserByLoginAsNoTrackingAsync(string login);

        Task SignOut(int userId, string fcmToken);

        void AddFcmToken(int userId, string fcmToken, int os);
    }
}