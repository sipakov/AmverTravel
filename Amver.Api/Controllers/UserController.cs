using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Amver.Api.CustomExceptionMiddleware;
using Amver.Api.Interfaces.Services;
using Amver.Domain.Constants;
using Amver.Domain.Dto;
using Amver.Domain.Entities;
using Amver.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using ValidationException = Amver.Api.CustomExceptionMiddleware.ValidationException;

namespace Amver.Api.Controllers
{
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly IStringLocalizer<AppResources> _stringLocalizer;


        public UserController(IUserService userService, IAuthService authService, IStringLocalizer<AppResources> stringLocalizer)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _stringLocalizer = stringLocalizer;
        }

        [HttpPost("create")]
        public async Task<ActionResult<UserDto>> CreateAsync([FromBody, Required] UserDto userDto)
        {
            if (!ModelState.IsValid) return BadRequest();

            return await _userService.CreateAsync(userDto);
        }

        //Todo remove
        [HttpGet("getMyMainProfileInfo")]
        public async Task<ActionResult<string>> GetMyMainProfileInfoAsync()
        {
            if (!ModelState.IsValid) return BadRequest();

            var login = HttpContext.User.Identity.Name;

            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            if (userAuth.ForceRelogin)
                throw new UnauthorizedException(string.Empty);

            return userAuth.UserName;
        }

        [Authorize]
        [DisableRequestSizeLimit]
        [HttpPost("uploadUserPhoto")]
        public async Task<ActionResult<BaseResult>> UploadUserPhotoAsync(
            [FromForm(Name = "file")] IFormFile uploadedPhoto)
        {
            if (!ModelState.IsValid) return BadRequest();

            var login = HttpContext.User.Identity.Name;

            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            if (userAuth == null || userAuth.IsBanned)
                throw new CustomExceptionMiddleware.ValidationException(_stringLocalizer["UserIsDeletedOrBanned"]);

            if (userAuth.ForceRelogin)
                throw new UnauthorizedException(string.Empty);

            return await _userService.UploadUserPhotoAsync(uploadedPhoto, login);
        }
        [Authorize]
        [HttpGet("getUserIcon")]
        public async Task<ActionResult<UserDto>> GetUserIconAsync()
        {
            if (!ModelState.IsValid) return BadRequest();

            var login = HttpContext.User.Identity.Name;

            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            if (userAuth.ForceRelogin)
                throw new UnauthorizedException(string.Empty);

            return await _userService.GetUserIconAsync(userAuth, login);
        }
        
        [HttpPost("ban")]
        public async Task<ActionResult<BaseResult>> BanAsync([FromBody, Required] BanDto banDto)
        {
            if (!ModelState.IsValid) return BadRequest();
            
            var login = HttpContext.User.Identity.Name;
            if (!string.IsNullOrEmpty(login))
            {
                var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

                banDto.UserId = userAuth.UserId;    
            }
            
            return await _userService.BanAsync(banDto);
        }
        
        [Authorize]
        [HttpGet("block")]
        public async Task<ActionResult<BaseResult>> BlockAsync([FromQuery, Required] int userId)
        {
            if (!ModelState.IsValid) return BadRequest();    

            var login = HttpContext.User.Identity.Name;

            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            if (userAuth == null)
                throw new CustomExceptionMiddleware.ValidationException(_stringLocalizer["UserIsDeletedOrBanned"]);

            if (userAuth.ForceRelogin)
                throw new UnauthorizedException(string.Empty);

            return await _userService.BlockAsync(userId, userAuth.UserId);
        }
        
        [Authorize]
        [HttpGet("unblock")]
        public async Task<ActionResult<BaseResult>> UnblockAsync([FromQuery, Required] int userId)
        {
            if (!ModelState.IsValid) return BadRequest();    

            var login = HttpContext.User.Identity.Name;

            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            if (userAuth == null)
                throw new CustomExceptionMiddleware.ValidationException(_stringLocalizer["UserIsDeletedOrBanned"]);

            if (userAuth.ForceRelogin)
                throw new UnauthorizedException(string.Empty);

            return await _userService.UnblockAsync(userId, userAuth.UserId);
        }
        
          
        [Authorize]
        [HttpGet("checkMyBlocked")]
        public async Task<ActionResult<UserToBlockedUser>> CheckMyBlockedAsync([FromQuery, Required] int userId)
        {
            if (!ModelState.IsValid) return BadRequest();    

            var login = HttpContext.User.Identity.Name;

            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            if (userAuth == null)
                throw new CustomExceptionMiddleware.ValidationException(_stringLocalizer["UserIsDeletedOrBanned"]);

            return await _userService.CheckMyBlockedAsync(userAuth.UserId, userId);
        }
        
        [Authorize]
        [HttpGet("checkForMyAndMeBlocked")]
        public async Task<ActionResult<List<UserToBlockedUser>>> CheckForMyAndMeBlockedAsync([FromQuery, Required] int userId)
        {
            if (!ModelState.IsValid) return BadRequest();    

            var login = HttpContext.User.Identity.Name;

            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            if (userAuth == null)
                throw new CustomExceptionMiddleware.ValidationException(_stringLocalizer["UserIsDeletedOrBanned"]);

            if (userAuth.ForceRelogin)
                throw new UnauthorizedException(string.Empty);

            return await _userService.CheckForMyAndMeBlockedAsync(userAuth.UserId, userId);
        }
    }
}