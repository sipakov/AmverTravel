using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.Domain.Dto;
using Amver.Domain.Entities;
using Amver.Domain.Enums;
using Amver.Domain.Models;
using Amver.EfCli;
using Amver.WebApi.CustomExceptionMiddleware;
using Amver.WebApi.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Localization;

namespace Amver.WebApi.Implementations.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUserService _userService;
        private readonly IPasswordEncryptor _passwordEncryptor;
        private readonly IAuthService _authService;
        private readonly IHostingEnvironment _appEnvironment;
        private readonly IContextFactory<ApplicationContext> _contextFactory;
        private readonly IStringLocalizer<AppResources> _stringLocalizer;


        public AccountService(IUserService userService, IPasswordEncryptor passwordEncryptor, IAuthService authService,
            IHostingEnvironment appEnvironment, IContextFactory<ApplicationContext> contextFactory, IStringLocalizer<AppResources> stringLocalizer)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _passwordEncryptor = passwordEncryptor ?? throw new ArgumentNullException(nameof(passwordEncryptor));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _appEnvironment = appEnvironment;
            _contextFactory = contextFactory;
            _stringLocalizer = stringLocalizer;
        }

        public async Task<TokenResponse> CreateAnAccount(UserCredential userCredential)
        {
            if (userCredential == null) throw new ArgumentNullException(nameof(userCredential));
            if (!IsValidPassword(userCredential.UserPassword))
                throw new ValidationException(_stringLocalizer["PasswordIsNotValid"]);
            if (!IsValidUserName(userCredential.Login))
                throw new ValidationException(_stringLocalizer["LoginIsNotValid"]);

            var newUser = new UserDto
            {
                Login = userCredential.Login,
                CountryId = default
            };
            var user = await _userService.CreateAsync(newUser);
            if (user != null)
            {
                var hashedPassword = _passwordEncryptor.Encrypt(userCredential.UserPassword);
                var _ = await _authService.CreateUserAuthenticationAsync(user.Id, hashedPassword, false, userCredential.FcmToken, userCredential.Os);
            }

            var claimIdentity = _authService.GetIdentity(user);

            var pathToDirectory = Path.Combine(_appEnvironment.WebRootPath, "Photos", "Users", $"{user.Login}");
            if (!Directory.Exists(pathToDirectory))
            {
                Directory.CreateDirectory(pathToDirectory);
            }

            var jwtToken = _authService.GetJwtToken(claimIdentity);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            return new TokenResponse {Token = encodedJwt, Login = user.Login};
        }

        public async Task<TokenResponse> ClassicLogin(UserCredential userCredential)
        {
            if (userCredential == null) throw new ArgumentNullException(nameof(userCredential));
            if (!IsValidPassword(userCredential.UserPassword))
                throw new ValidationException(_stringLocalizer["PasswordIsNotValid"]);
            if (!IsValidUserName(userCredential.Login))
                throw new ValidationException(_stringLocalizer["LoginIsNotValid"]);

            var user = await _userService.GetUserByLoginAsync(userCredential.Login);

            if (user == null)
                throw new ValidationException(_stringLocalizer["PasswordOrLoginIsWrong"]);

            var hashedPassword = _passwordEncryptor.Encrypt(userCredential.UserPassword);
            var isValidPassword = await _authService.IsValidPasswordAsync(user.Id, hashedPassword);

            if (!isValidPassword)
                throw new ValidationException(_stringLocalizer["PasswordOrLoginIsWrong"]);

            var claimIdentity = _authService.GetIdentity(user);

            var jwtToken = _authService.GetJwtToken(claimIdentity);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            if (string.IsNullOrEmpty(userCredential.FcmToken))
                return new TokenResponse {Token = encodedJwt, Login = user.Login};
            
            var userToFcmToken = new UserToFcmToken
            {
                UserId = user.Id,
                FcmToken = userCredential.FcmToken,
                LastSignIn = DateTime.UtcNow,
                LastVisit = DateTime.UtcNow,
                IsInApp = true,
                Os = userCredential.Os
            };
            using (var context = _contextFactory.CreateContext())
            {
                var userToFcmTokenExist =
                    await context.UserToFcmTokens.FindAsync(userToFcmToken.UserId, userToFcmToken.FcmToken);
                if (userToFcmTokenExist == null)
                {
                    await context.UserToFcmTokens.AddAsync(userToFcmToken);
                }
                else
                {
                    userToFcmTokenExist.IsInApp = true;
                    userToFcmTokenExist.LastSignIn = DateTime.UtcNow;
                    userToFcmTokenExist.LastVisit = DateTime.UtcNow;
                }

                await context.SaveChangesAsync();
            }
            
            return new TokenResponse {Token = encodedJwt, Login = user.Login};
        }

        public async Task<TokenResponse> FbLogin(string marker)
        {
            if (string.IsNullOrEmpty(marker))
                throw new ArgumentException("Value cannot be null or empty.", nameof(marker));

            var facebookResponse = await _authService.GetFacebookDataByToken(marker);

            if (facebookResponse == null) throw new ArgumentNullException(nameof(facebookResponse));

            var user = await _userService.GetUserByLoginAsync(facebookResponse.id);
            byte[] imageBytes = null;
            if (!string.IsNullOrEmpty(facebookResponse.picture.data.url))
            {
                using (var webClient = new WebClient())
                {
                    imageBytes = webClient.DownloadData(facebookResponse.picture.data.url);
                }
            }
            if (user == null)
            {
                var newUser = new UserDto
                {
                    Email = facebookResponse.email,
                    FirstName = facebookResponse.first_name,
                    LastName = facebookResponse.last_name,
                    GenderId =1,// (int) (Genders) Enum.Parse(typeof(Genders), facebookResponse.gender, true),
                    Login = facebookResponse.id,
                    CountryId = default,
                    BirthDay = DateTime.Now,// facebookResponse.birthday,
                };
                var createdUser = await _userService.CreateAsync(newUser);
                if (createdUser != null)
                {
                    var _ = await _authService.CreateUserAuthenticationAsync(createdUser.Id, null, true, String.Empty, default);
                }
                user = await _userService.GetUserByLoginAsync(facebookResponse.id);
            }

            var uploadLocation = string.Empty;
            if (OperatingSystem.IsWindows())
                uploadLocation = Path.Combine(_appEnvironment.WebRootPath, $"Photos\\Users\\{user.Login}\\");
            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                uploadLocation = Path.Combine(_appEnvironment.WebRootPath, "Photos", "Users", $"{user.Login}",
                    $"{user.Login}_main.jpeg");
            if (string.IsNullOrEmpty(uploadLocation))
                throw new ArgumentNullException(nameof(uploadLocation));
            var pathToDirectory = Path.Combine(_appEnvironment.WebRootPath, "Photos", "Users", $"{user.Login}");
            if (!Directory.Exists(pathToDirectory))
            {
                Directory.CreateDirectory(pathToDirectory);
            }

            using (var stream = new FileStream(uploadLocation, FileMode.Create))
            {
                if (imageBytes != null) await stream.WriteAsync(imageBytes, 0, imageBytes.Length);
            }

            var claimIdentity = _authService.GetIdentity(user);

            var jwtToken = _authService.GetJwtToken(claimIdentity);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            return new TokenResponse {Token = encodedJwt, Login = user?.Login};
        }

        private bool IsValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Value cannot be null or empty.", nameof(password));

            return Regex.IsMatch(password, AuthOptions.PatternToValidatePassword);
        }

        private bool IsValidUserName(string login)
        {
            if (string.IsNullOrEmpty(login))
                throw new ArgumentException("Value cannot be null or empty.", nameof(login));

            return Regex.IsMatch(login, AuthOptions.PatternToValidateLogin);
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
}