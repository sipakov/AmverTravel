using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amver.Api.Interfaces.Services;
using Amver.Api.Interfaces.Storages;
using Amver.Domain.Constants;
using Amver.Domain.Entities;
using Amver.Domain.Dto;
using Amver.Domain.Models;
using Amver.EfCli;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using ValidationException = Amver.Api.CustomExceptionMiddleware.ValidationException;

namespace Amver.Api.Implementations.Services
{
    public class UserService : IUserService
    {
        private readonly IUserStorage _userStorage;
        private readonly ILogger<UserService> _logger;
        private readonly IHostingEnvironment _appEnvironment;
        private readonly IContextFactory<ApplicationContext> _contextFactory;

        public UserService(IUserStorage userStorage, ILogger<UserService> logger, IHostingEnvironment appEnvironment, IContextFactory<ApplicationContext> contextFactory)
        {
            _userStorage = userStorage ?? throw new ArgumentNullException(nameof(userStorage));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appEnvironment = appEnvironment ?? throw new ArgumentNullException(nameof(appEnvironment));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        public async Task<UserDto> CreateAsync(UserDto userDto)
        {
            if (userDto == null) throw new ArgumentNullException(nameof(userDto));
            if (string.IsNullOrEmpty(userDto.Login)) throw new ArgumentNullException(nameof(userDto.Login));

            var user = new User
            {
                Email = userDto.Email,
                Login = userDto.Login,
                FirstName = userDto.FirstName ?? "Unknown",
                LastName = userDto.LastName,
                Comment = userDto.Comment,
                CreatedDate = DateTime.UtcNow,
                GenderId = userDto.GenderId == default ? 4 : userDto.GenderId,
                BirthDay = userDto.BirthDay
            };

            var userFromStorage = await _userStorage.GetUserByLoginAsync(user.Login);
            if (userFromStorage != null)
            {
                _logger.LogError($"User with current login {user.Login} is exist");
                throw new CustomExceptionMiddleware.ValidationException($"User with current login {user.Login} is exist");
            }

            var insertedUserId = await _userStorage.CreateAsync(user);
            userDto.Id = insertedUserId;
            return userDto;
        }
        
        public async Task<UserDto> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Value cannot be null or empty.", nameof(email));

            var user = await _userStorage.GetUserByEmailAsync(email);

            if (user == null) return null;

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsBanned = user.IsBanned,
                IsDeleted = user.DeletedDate != null,
                CreatedDate = user.CreatedDate
            };
            return userDto;
        }

        public async Task<UserDto> GetUserByLoginAsync(string login)
        {
            if (string.IsNullOrEmpty(login))
                throw new ArgumentException("Value cannot be null or empty.", nameof(login));

            var user = await _userStorage.GetUserByLoginAsync(login);

            if (user == null) return null;

            var userDto = new UserDto
            {
                Id = user.Id,
                Login = user.Login,
                Email = user.Email,
                FirstName = user.FirstName,
                BirthDay = user.BirthDay,
                IsBanned = user.IsBanned,
                IsDeleted = user.DeletedDate != null,
                CreatedDate = user.CreatedDate
            };
            return userDto;
        }

        public async Task<BaseResult> UpdateUserAfterFinishedRegistrationAsync(UserDto userDto)
        {
            if (userDto == null) throw new ArgumentException("Value cannot be null or empty.", nameof(userDto));
            var validEmail = GetValidEmail(userDto.Email);
            if (validEmail == null) throw new CustomExceptionMiddleware.ValidationException("Email not valid");

            var user = new User
            {
                Id = userDto.Id,
                Email = validEmail.ToString(),
                BirthDay = userDto.BirthDay,
                FirstName = userDto.FirstName,
                GenderId = userDto.GenderId
            };
            return await _userStorage.UpdateAsync(user);
        }

        public async Task<BaseResult> UploadUserPhotoAsync(IFormFile uploadedPhoto, string userLogin)
        {
            if (uploadedPhoto == null) throw new ArgumentNullException(nameof(uploadedPhoto));
            if (string.IsNullOrEmpty(userLogin))
                throw new ArgumentException("Value cannot be null or empty.", nameof(userLogin));
            var uploadLocationMain = string.Empty;
            var uploadLocationIcon = string.Empty;
            var uploadLocationConversation = string.Empty;
            if (OperatingSystem.IsWindows())
                uploadLocationMain = Path.Combine(_appEnvironment.WebRootPath, $"Photos\\Users\\{userLogin}\\");
            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                var newGuid = Guid.NewGuid();
                uploadLocationMain = Path.Combine(_appEnvironment.WebRootPath, "Photos", "Users", $"{userLogin}",
                    $"{userLogin}_{newGuid}_main.jpeg");
                uploadLocationIcon = Path.Combine(_appEnvironment.WebRootPath, "Photos", "Users", $"{userLogin}",
                    $"{userLogin}_{newGuid}_icon.jpeg");
                uploadLocationConversation = Path.Combine(_appEnvironment.WebRootPath, "Photos", "Users", $"{userLogin}",
                    $"{userLogin}_{newGuid}_conversation.jpeg");
            }

            var pathToDirectory = Path.Combine(_appEnvironment.WebRootPath, "Photos", "Users", $"{userLogin}");
            if (!Directory.Exists(pathToDirectory))
            {
                Directory.CreateDirectory(pathToDirectory);
            }
            var di = new DirectoryInfo(pathToDirectory);
            foreach (var file in di.GetFiles())
            {
                file.Delete(); 
            }

            using (var fileStream = new FileStream(uploadLocationMain, FileMode.Create))
            {
                await uploadedPhoto.CopyToAsync(fileStream);
            }

            const int maxWidthHeightIcon = 40;
            SaveResizedImage(uploadedPhoto, uploadLocationIcon, maxWidthHeightIcon );
            
            const int maxWidthHeightConversation = 120;
            SaveResizedImage(uploadedPhoto, uploadLocationConversation, maxWidthHeightConversation);

            return new BaseResult();
        }

        public async Task<UserDto> GetUserByIdAsync(int userId)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            var user = await _userStorage.GetUserByIdAsync(userId);

            if (user == null)
                return null;

            var pathToDirectory = Path.Combine(_appEnvironment.WebRootPath, "Photos", "Users", $"{user.Login}");
            
            var targetImages = new List<string>();
            if (Directory.Exists(pathToDirectory))
            {
                targetImages = Directory.GetFiles(pathToDirectory, "*_main.jpeg").ToList();
            }
            var userDto = new UserDto
            {
                Id = user.Id,
                Login = user.Login,
                FirstName = user.FirstName,
                BirthDay = user.BirthDay,
                IsBanned = user.IsBanned,
                IsDeleted = user.DeletedDate != null,
                Comment = user.Comment,
                CreatedDate = user.CreatedDate,
                City = user.City?.Name,
                CityId = user.CityId,
                Country = user.City?.Country.Name,
                Email = user.Email,
                GenderId = user.GenderId ?? default,
                // UserImage = File.Exists(Path.Combine(pathToDirectory, $"{user.Login}_main.jpeg"))
                //     ? File.ReadAllBytes(
                //         Path.Combine(pathToDirectory, $"{user.Login}_main.jpeg"))
                //     : null,
                UserImageUri = targetImages.Any() ? $"https://www.amver.net/Photos/Users/{user.Login}/{Path.GetFileName(targetImages.First())}" : null

            };
            return userDto;
        }
        
        public async Task<UserDto> GetUserForUpdateByIdAsync(int userId)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            var user = await _userStorage.GetUserByIdAsync(userId);

            if (user == null)
                return null;
            
            var userDto = new UserDto
            {
                Id = user.Id,
                Login = user.Login,
                FirstName = user.FirstName,
                BirthDay = user.BirthDay,
                IsBanned = user.IsBanned,
                IsDeleted = user.DeletedDate != null,
                Comment = user.Comment,
                CreatedDate = user.CreatedDate,
                City = user.City?.Name,
                CityId = user.CityId,
                Country = user.City?.Country.Name,
                Email = user.Email,
                GenderId = user.GenderId ?? default
            };
            return userDto;
        }

        public async Task<UserDto> GetUserIconAsync(UserAuth userAuth, string userLogin)
        {
            if (userLogin == null) throw new ArgumentNullException(nameof(userLogin));
            if (userAuth == null)
                throw new ArgumentException("Value cannot be null or empty.", nameof(userAuth));

            var pathToDirectory = Path.Combine(_appEnvironment.WebRootPath, "Photos", "Users", $"{userLogin}");
            var targetImages = new List<string>();
            if (Directory.Exists(pathToDirectory))
            {
                targetImages = Directory.GetFiles(pathToDirectory, "*_icon.jpeg").ToList();
            }
            var userDto = new UserDto
            {
                FirstName = userAuth.UserName,
                Email = userAuth.Email,
                GenderId = userAuth.Gender ?? default,
                // UserImage = File.Exists(Path.Combine(pathToDirectory, $"{userLogin}_icon.jpeg"))
                //     ? await File.ReadAllBytesAsync(
                //         Path.Combine(pathToDirectory, $"{userLogin}_icon.jpeg"))
                //     : null,
                UserImageUri = targetImages.Any() ? $"https://www.amver.net/Photos/Users/{userLogin}/{Path.GetFileName(targetImages.First())}" : Path.Combine("https://www.amver.net", "images", "userAccountIcon.png")
            };

            return userDto;
        }

        public async Task<BaseResult> UpdateAsync(UserDto userDto)
        {
            if (userDto == null) throw new ArgumentNullException(nameof(userDto));
            var validEmail = GetValidEmail(userDto.Email);
            if (validEmail == null) throw new CustomExceptionMiddleware.ValidationException("Email not valid");

            var user = new User
            {
                Id = userDto.Id,
                FirstName = userDto.FirstName,
                CityId = userDto.CityId < 1 ? null : userDto.CityId,
                BirthDay = userDto.BirthDay,
                Email = userDto.Email,
                GenderId = userDto.GenderId,
                Comment = userDto.Comment
            };

            return await _userStorage.UpdateAsync(user);
        }

        public async Task<BaseResult> RemoveAsync(int userId)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            return await _userStorage.RemoveAsync(userId);
        }

        private void SaveResizedImage(IFormFile uploadedImage, string saveLocation, int maxWidthHeight)
        {
            if (uploadedImage == null) throw new ArgumentNullException(nameof(uploadedImage));
            if (string.IsNullOrEmpty(saveLocation))
                throw new ArgumentException("Value cannot be null or empty.", nameof(saveLocation));
            if (maxWidthHeight <= 0) throw new ArgumentOutOfRangeException(nameof(maxWidthHeight));
            
            using (var outputStream = new FileStream(saveLocation, FileMode.Create))
            {
                var stream = uploadedImage.OpenReadStream();
                using (var image = Image.Load(stream, out _))
                {
                    var ratioX = (double) maxWidthHeight / image.Width;
                    var ratioY = (double) maxWidthHeight / image.Height;
                    var ratio = Math.Max(ratioX, ratioY);
                    var newWidth = (int) (image.Width * ratio);
                    var newHeight = (int) (image.Height * ratio);
                    var encoder = new JpegEncoder
                    {
                    Quality = 100
                    };
                    image.Mutate(c => c.Resize(newWidth, newHeight));
                    image.SaveAsJpeg(outputStream, encoder);
                }
            }
        }
        
        private static MailAddress GetValidEmail(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return mailAddress;
            }
            catch
            {
                return null;
            }
        }
        
        private bool IsValidUserName(string login)
        {
            if (string.IsNullOrEmpty(login)) throw new ArgumentException("Value cannot be null or empty.", nameof(login));

            return Regex.IsMatch(login, AuthOptions.PatternToValidateName);
        }

        public async Task<BaseResult> BanAsync(BanDto banDto)
        {
            if (banDto == null) throw new ArgumentNullException(nameof(banDto));

            await using var context = _contextFactory.CreateContext();
            int objectionableUserId = default;
            if (!banDto.ObjectionableUserId.HasValue && banDto.ObjectionableTripId.HasValue)
            {
                objectionableUserId = (await context.Trips.AsNoTracking()
                    .FirstAsync(x => x.Id == banDto.ObjectionableTripId)).UserId;
            }
            if (banDto.ObjectionableUserId.HasValue && !banDto.ObjectionableTripId.HasValue)
            {
                objectionableUserId = banDto.ObjectionableUserId.Value;
            }
            await context.ObjectionableContents.AddAsync(new ObjectionableContent
            {
                UserId = banDto.UserId,
                ObjectionableUserId = objectionableUserId,
                ObjectionableReasonId = banDto.ObjectionableReasonId,
                TripId = banDto.ObjectionableTripId ?? default,
                Comment = banDto.Comment,
                BanDate = DateTime.UtcNow
            });
            // var objectionableUser = await context.Users.FirstAsync(x => x.Id == objectionableUserId);
            // objectionableUser.IsBanned = true;
            // var userConversationsOne = context.Conversations.Where(x =>
            //     x.UserId == banDto.UserId && x.Trip.UserId == objectionableUserId).Select(x=>x.Id);
            // var userConversationsTwo = context.Conversations.Where(x =>
            //     x.UserId == objectionableUserId && x.Trip.UserId == banDto.UserId).Select(x=>x.Id);
            // var targetConversations = userConversationsOne.Union(userConversationsTwo).ToList();
            // foreach (var targetConversation in targetConversations)
            // {
            //     context.Conversations.First(x => x.Id == targetConversation).IsDeleted = true;
            // }
            await context.SaveChangesAsync();
            return new BaseResult();
        }

        public async Task<BaseResult> BlockAsync(int targetUserIdToBlock, int userId)
        {
            if (targetUserIdToBlock <= 0) throw new ArgumentOutOfRangeException(nameof(targetUserIdToBlock));
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            
            var newUserToBlockedUser = new UserToBlockedUser
            {
                UserId = userId,
                BlockedUserId = targetUserIdToBlock,
                BlockingDate = DateTime.UtcNow
            };
            using (var context = _contextFactory.CreateContext())
            {
                var userToBlockedUser = await context.UserToBlockedUsers.FindAsync(userId, targetUserIdToBlock);
                if (userToBlockedUser != null) return new BaseResult();
                await context.UserToBlockedUsers.AddAsync(newUserToBlockedUser);
                await context.SaveChangesAsync();
                return new BaseResult();
            }
        }
        
        public async Task<BaseResult> UnblockAsync(int targetUserIdToUnblock, int userId)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            if (targetUserIdToUnblock <= 0) throw new ArgumentOutOfRangeException(nameof(targetUserIdToUnblock));

            using (var context = _contextFactory.CreateContext())
            {
                var user = await context.UserToBlockedUsers.FindAsync(userId, targetUserIdToUnblock);
                if (user == null) return new BaseResult();
                context.UserToBlockedUsers.Remove(user);
                await context.SaveChangesAsync();
                return new BaseResult();
            }
        }

        public async Task<UserToBlockedUser> CheckMyBlockedAsync(int myUserId, int targetUserId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                return await context.UserToBlockedUsers.FindAsync(myUserId, targetUserId);
            }
        }

        public async Task<List<UserToBlockedUser>> CheckForMyAndMeBlockedAsync(int myUserId, int targetUserId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                var result = await context.UserToBlockedUsers.Where(x =>
                    (x.UserId == targetUserId && x.BlockedUserId == myUserId) ||
                    (x.UserId == myUserId && x.BlockedUserId == targetUserId)).ToListAsync();
                return result;
            }
        }
    }

    public static class OperatingSystem
    {
        public static bool IsWindows() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsMacOS() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool IsLinux() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }
}