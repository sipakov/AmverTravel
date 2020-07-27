using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Amver.Domain.Dto;
using Amver.Domain.Entities;
using Amver.Domain.Models;

namespace Amver.Api.Interfaces.Services
{
    public interface IAuthService
    {
        Task<FacebookResponse> GetFacebookDataByToken(string token);

        ClaimsIdentity GetIdentity(UserDto userDto);

        JwtSecurityToken GetJwtToken(ClaimsIdentity claimsIdentity);

        Task<UserAuthentication> CreateUserAuthenticationAsync(int userId, string hashedPassword, bool isEmailConfirmed, string fcmToken, int os);
        
        Task<bool> IsValidPasswordAsync(int userId, string hashedPassword);

        Task<UserAuth> GetActiveUserByLoginAsNoTrackingAsync(string login);
    }
}