using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.Domain.Dto;
using Amver.Domain.Models;
using Amver.EfCli;
using Amver.WebApi.CustomExceptionMiddleware;
using Amver.WebApi.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ValidationException = Amver.WebApi.CustomExceptionMiddleware.ValidationException;

namespace Amver.WebApi.Controllers
{
    [Route("[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly IProfileService _profileService;
        private readonly IContextFactory<ApplicationContext> _contextFactory;
        private readonly IStringLocalizer<AppResources> _stringLocalizer;


        public ProfileController(IUserService userService, IAuthService authService, IProfileService profileService, IContextFactory<ApplicationContext> contextFactory, IStringLocalizer<AppResources> stringLocalizer)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _stringLocalizer = stringLocalizer;
        }

        [Authorize]
        [HttpGet("get")]
        public async Task<ActionResult<UserDto>> GetMyProfileAsync()
        {
            if (!ModelState.IsValid) return BadRequest();
            
            var login = HttpContext.User.Identity.Name;

            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            if (userAuth.ForceRelogin)
                throw new UnauthorizedException(string.Empty);

            return await _userService.GetUserByIdAsync(userAuth.UserId);
        }
        
        [HttpGet("getById")]
        public async Task<ActionResult<UserDto>> GetProfileByIdAsync([FromQuery, Required] int userId)
        {
            if (!ModelState.IsValid) return BadRequest();
            
            return await _userService.GetUserByIdAsync(userId);
        }
        [Authorize]
        [HttpGet("getAuthById")]
        public async Task<ActionResult<UserDto>> GetAuthProfileByIdAsync([FromQuery, Required] int userId)
        {
            if (!ModelState.IsValid) return BadRequest();
            var login = HttpContext.User.Identity.Name;
            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            await ValidateToBlock(userAuth.UserId, userId);
            
            return await _userService.GetUserByIdAsync(userId);
        }

        private async Task ValidateToBlock(int myUserId, int targetUserId)
        {
            if (myUserId <= 0) throw new ArgumentOutOfRangeException(nameof(myUserId));
            if (targetUserId <= 0) throw new ArgumentOutOfRangeException(nameof(targetUserId));
            using (var context = _contextFactory.CreateContext())
            {
                var userToBlockedUser = await context.UserToBlockedUsers.FindAsync(targetUserId, myUserId);
                if (userToBlockedUser != null)
                {
                    throw new ValidationException(_stringLocalizer["UserIsDeletedOrBanned"]);  
                }
            }
        }
        
        [Authorize]
        [HttpGet("removeImage")]
        public async Task<ActionResult<BaseResult>> RemoveProfileImageAsync()
        {
            if (!ModelState.IsValid) return BadRequest();
            
            var login = HttpContext.User.Identity.Name;

            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            if (userAuth.ForceRelogin)
                throw new UnauthorizedException(string.Empty);

            return _profileService.RemoveProfileImage(login);
        }
        
        [Authorize]
        [HttpPost("update")]
        public async Task<ActionResult<BaseResult>> UpdateAsync([FromBody, BindRequired] UserDto userDto)
        {
            if (!ModelState.IsValid) return BadRequest();
            
            var login = HttpContext.User.Identity.Name;
            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);
            if (userAuth.ForceRelogin)
                throw new UnauthorizedException(string.Empty);

            userDto.Id = userAuth.UserId;

            return await _userService.UpdateAsync(userDto);
        }
        
        [Authorize]
        [HttpGet("remove")]
        public async Task<ActionResult<BaseResult>> RemoveAsync()
        {
            if (!ModelState.IsValid) return BadRequest();
            
            var login = HttpContext.User.Identity.Name;
            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            if (userAuth.ForceRelogin)
                throw new UnauthorizedException(string.Empty);

            return await _userService.RemoveAsync(userAuth.UserId);
        }
        
        [Authorize]
        [HttpGet("getForUpdate")]
        public async Task<ActionResult<UserDto>> GetMyProfileForUpdateAsync()
        {
            if (!ModelState.IsValid) return BadRequest();
            
            var login = HttpContext.User.Identity.Name;

            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            if (userAuth.ForceRelogin)
                throw new UnauthorizedException(string.Empty);

            return await _userService.GetUserForUpdateByIdAsync(userAuth.UserId);
        }
    }
}