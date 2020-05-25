using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.Domain.Dto;
using Amver.Domain.Entities;
using Amver.Domain.Models;
using Amver.WebApi.Interfaces.Services;
using Amver.WebApi.Interfaces.Storages;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Amver.WebApi.Implementations.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthStorage _authStorage;

        public AuthService(IAuthStorage authStorage)
        {
            _authStorage = authStorage ?? throw new ArgumentNullException(nameof(authStorage));
        }

        public async Task<FacebookResponse> GetFacebookDataByToken(string token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));

            var fullUrl = Url.GetProfileByFbMarker + token;
            var client = new HttpClient();
            var response = await client.GetAsync(fullUrl);
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<FacebookResponse>(content);
        }

        public ClaimsIdentity GetIdentity(UserDto userDto)
        {
            if (userDto == null) throw new ArgumentNullException(nameof(userDto));

            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userDto.Login),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, "default")
            };
            var claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            return claimsIdentity;
        }

        public JwtSecurityToken GetJwtToken(ClaimsIdentity claimsIdentity)
        {
            if (claimsIdentity == null)
                throw new ArgumentNullException(nameof(claimsIdentity));
            var jwt = new JwtSecurityToken(issuer: AuthOptions.Issuer,
                audience: AuthOptions.Audience,
                notBefore: DateTime.UtcNow,
                claims: claimsIdentity.Claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(AuthOptions.LifeTimeMinutes)),
                signingCredentials: new SigningCredentials(AuthOptions.SymmetricSecurityKey,
                    SecurityAlgorithms.HmacSha256));
            return jwt;
        }

        public async Task<UserAuthentication> CreateUserAuthenticationAsync(int userId, string hashedPassword, bool isEmailConfirmed, string fcmToken, int os)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            
            var userAuth = new UserAuthentication
            {
                UserId = userId,
                HashedPassword = hashedPassword,
                IsEmailConfirmed = isEmailConfirmed
            };

            return await _authStorage.CreateUserAuthenticationAsync(userAuth, fcmToken, os);
        }

        public async Task<bool> IsValidPasswordAsync(int userId, string hashedPassword)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            if (string.IsNullOrEmpty(hashedPassword))
                throw new ArgumentException("Value cannot be null or empty.", nameof(hashedPassword));

            return await _authStorage.IsValidPasswordAsync(userId, hashedPassword);
        }

        public async Task<UserAuth> GetActiveUserByLoginAsNoTrackingAsync(string login)
        {
            if (string.IsNullOrEmpty(login))
                throw new ArgumentException("Value cannot be null or empty.", nameof(login));

            return await _authStorage.GetActiveUserByLoginAsNoTrackingAsync(login);
        }
    }
}