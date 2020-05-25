using System.Threading.Tasks;
using Amver.Domain.Entities;
using Amver.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace Amver.WebApi.Interfaces.Storages
{
    public interface IUserStorage
    {
        Task<int> CreateAsync(User user);

         Task<User> GetUserByEmailAsync(string email);
         
         Task<User> GetUserByLoginAsync(string login);

         Task<BaseResult> UpdateAsync(User userToUpdate);

         Task<User> GetUserByIdAsync(int userId);
         
         Task<BaseResult> RemoveAsync(int userId);
        
    }
}
