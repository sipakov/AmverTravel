using System;
using System.Linq;
using System.Threading.Tasks;
using Amver.Api.Interfaces.Storages;
using Amver.Domain.Constants;
using Amver.Domain.Entities;
using Amver.Domain.Models;
using Amver.EfCli;
using Microsoft.EntityFrameworkCore;

namespace Amver.Api.Implementations.Storages
{
    public class AuthStorage : IAuthStorage
    {
        private readonly IContextFactory<ApplicationContext> _contextFactory;

        public AuthStorage(IContextFactory<ApplicationContext> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        public async Task<UserAuthentication> CreateUserAuthenticationAsync(UserAuthentication userAuthentication, string fcmToken, int os)
        {
            if (userAuthentication == null) throw new ArgumentNullException(nameof(userAuthentication));

            using (var context = _contextFactory.CreateContext())
            {
                var userAuth = await context.UserAuthentications.AddAsync(userAuthentication);
                if (!string.IsNullOrEmpty(fcmToken))
                {
                    var userToFcmToken = new UserToFcmToken
                    {
                        UserId = userAuthentication.UserId,
                        FcmToken = fcmToken,
                        LastSignIn = DateTime.UtcNow,
                        LastVisit = DateTime.UtcNow,
                        IsInApp = true,
                        Os = os
                    };
                    await context.UserToFcmTokens.AddAsync(userToFcmToken);  
                }
                await context.SaveChangesAsync();
                return userAuth.Entity;
            }
        }

        public async Task<bool> IsValidPasswordAsync(int userId, string hashedPassword)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            if (string.IsNullOrEmpty(hashedPassword))
                throw new ArgumentException("Value cannot be null or empty.", nameof(hashedPassword));

            using (var context = _contextFactory.CreateContext())
            {
                var userAuth = await context.UserAuthentications.FirstOrDefaultAsync(x =>
                    x.UserId == userId && x.HashedPassword == hashedPassword);

                return userAuth != null;
            }
        }

        public async Task<UserAuth> GetActiveUserByLoginAsNoTrackingAsync(string login)
        {
            if (string.IsNullOrEmpty(login))
                throw new ArgumentException("Value cannot be null or empty.", nameof(login));

            using (var context = _contextFactory.CreateContext())
            {
                var userAuth = await context.Users.Where(x => x.Login == login && x.DeletedDate == null).AsNoTracking()
                    .Join(context.UserAuthentications, x => x.Id, y => y.UserId,
                        (user, authentication) => new UserAuth
                        { UserId = user.Id, ForceRelogin = authentication.ForceRelogin, UserName = user.FirstName, Email = user.Email, Gender = user.GenderId, IsBanned = user.IsBanned}).FirstOrDefaultAsync();
                return userAuth;
            }
        }

        public async Task SignOut(int userId, string fcmTokenStr)
        {
            using (var context = _contextFactory.CreateContext())
            {
                var fcmToken = context.UserToFcmTokens.FirstOrDefault(x => x.UserId == userId && x.FcmToken == fcmTokenStr);
                if (fcmToken != null)
                {
                    fcmToken .IsInApp = false;
                }
                await context.SaveChangesAsync();
            }
        }

        public async void AddFcmToken(int userId, string fcmToken, int os)
        {
            await using var context = _contextFactory.CreateContext();
            if (string.IsNullOrEmpty(fcmToken))
                return;
            
            var userToFcmToken = await context.UserToFcmTokens.FindAsync(userId, fcmToken);
            if (userToFcmToken == null)
            {
                var user = context.Users.AsNoTracking().First(x => x.Id == userId);
                var newUserToFcmToken = new UserToFcmToken
                {
                    FcmToken = fcmToken,
                    IsInApp = true,
                    LastSignIn = user.CreatedDate + TimeSpan.FromMinutes(AuthOptions.LifeTimeMinutes) > DateTime.UtcNow ? user.CreatedDate : DateTime.UtcNow,
                    LastVisit = DateTime.UtcNow,
                    UserId = userId
                };
                await context.UserToFcmTokens.AddAsync(newUserToFcmToken);
            }
            else
            {
                userToFcmToken.LastVisit = DateTime.UtcNow;
            }
            await context.SaveChangesAsync();
        }
    }
}