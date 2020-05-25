using System;
using System.Linq;
using System.Threading.Tasks;
using Amver.Domain.Dto;
using Amver.Domain.Entities;
using Amver.Domain.Models;
using Amver.EfCli;
using Amver.WebApi.Interfaces.Storages;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Amver.WebApi.Implementations.Storages
{
    public class UserStorage : IUserStorage
    {
        private readonly ILogger<UserStorage> _logger;

        private readonly IContextFactory<ApplicationContext> _contextFactory;

        public UserStorage(IContextFactory<ApplicationContext> contextFactory, ILogger<UserStorage> logger)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public async Task<int> CreateAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            try
            {
                _logger.LogInformation($"Trying to create user");
                using (var context = _contextFactory.CreateContext())
                {
                    await context.Users.AddAsync(user);
                    await context.SaveChangesAsync();
                
                    return user.Id;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to create user", ex);
                throw;
            }
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Value cannot be null or empty.", nameof(email));

            using (var context = _contextFactory.CreateContext())
            {
                var user = await context.Users.FirstOrDefaultAsync(x => x.Email == email);

                return user;
            }
        }
        
        public async Task<User> GetUserByLoginAsync(string login)
        {
            if (string.IsNullOrEmpty(login))
                throw new ArgumentException("Value cannot be null or empty.", nameof(login));

            using (var context = _contextFactory.CreateContext())
            {
                var user = await context.Users.Include(x=>x.City).Include(x=>x.Country).FirstOrDefaultAsync(x => x.Login== login);

                return user;
            }
        }
        
        public async Task<BaseResult> UpdateAsync(User userToUpdate)
        {
            if (userToUpdate == null) throw new ArgumentNullException(nameof(userToUpdate));
            
            using (var context = _contextFactory.CreateContext())
            {
                var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userToUpdate.Id);
                if (user == null) throw new ArgumentNullException(nameof(user));
                
                user.FirstName = userToUpdate.FirstName;
                user.BirthDay = userToUpdate.BirthDay;
                user.Email = userToUpdate.Email;
                user.GenderId = userToUpdate.GenderId;
                user.CityId = userToUpdate.CityId;
                user.Comment = userToUpdate.Comment;
                
                await context.SaveChangesAsync();

                return new BaseResult();
            }
        }

        public async Task<User> UploadUserPhotoAsync(string uploadedPhotoFullPath, int userId)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            if (string.IsNullOrEmpty(uploadedPhotoFullPath))
                throw new ArgumentException("Value cannot be null or empty.", nameof(uploadedPhotoFullPath));

            using (var context = _contextFactory.CreateContext())
            {
                var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId); 
                if (user == null) throw new ArgumentNullException(nameof(user));
                //implement only once photo
                await context.SaveChangesAsync();

                return user;
            }
        }
        public async Task<User> GetUserByIdAsync(int userId)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            
            using (var context = _contextFactory.CreateContext())
            {
                var user = await context.Users.AsNoTracking().Include(x=>x.City).ThenInclude(x=>x.Country).FirstOrDefaultAsync(x => x.Id == userId && x.DeletedDate == null);

                return user;
            }
        }

        public async Task<BaseResult> RemoveAsync(int userId)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            using (var context = _contextFactory.CreateContext())
            {
                var targetUser = await context.Users.FirstOrDefaultAsync(x => x.Id == userId);
                if (targetUser == null) throw new ArgumentNullException(nameof(userId));
                
                targetUser.DeletedDate = DateTime.UtcNow;
                var userToFcmTokens = await context.UserToFcmTokens.Where(x => x.UserId == userId).ToListAsync();
                if (userToFcmTokens.Any())
                {
                    foreach (var userToFcmToken in userToFcmTokens)
                    {
                        userToFcmToken.IsInApp = false;
                    }
                }
                await context.SaveChangesAsync();
            }
            return new BaseResult();
        }
    }
}