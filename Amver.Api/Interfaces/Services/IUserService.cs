using System.Collections.Generic;
using System.Threading.Tasks;
using Amver.Domain.Dto;
using Amver.Domain.Entities;
using Amver.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace Amver.Api.Interfaces.Services
{
    public interface IUserService
    {
       Task<UserDto> CreateAsync(UserDto userDto);

       Task<UserDto> GetUserByEmailAsync(string email);
       
       Task<UserDto> GetUserByLoginAsync(string login);

       Task<BaseResult> UpdateUserAfterFinishedRegistrationAsync(UserDto userDto);
       
       Task<BaseResult> UploadUserPhotoAsync(IFormFile uploadedPhotoStr, string userLogin);

       Task<UserDto> GetUserByIdAsync(int userId);
       
       Task<UserDto> GetUserForUpdateByIdAsync(int userId);

       Task<UserDto> GetUserIconAsync(UserAuth userAuth, string userLogin);
       Task<BaseResult> UpdateAsync(UserDto userDto);
       
       Task<BaseResult> RemoveAsync(int userId);
       Task<BaseResult> BanAsync(BanDto banDto);
       Task<BaseResult> BlockAsync(int targetUserIdToBlock, int userId);
       Task<UserToBlockedUser> CheckMyBlockedAsync(int myUserId, int targetUserId);
       
       Task<List<UserToBlockedUser>> CheckForMyAndMeBlockedAsync(int myUserId, int targetUserId);
       
       Task<BaseResult> UnblockAsync(int targetUserIdToUnblock, int userId);
    }
}
