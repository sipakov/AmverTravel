using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.Domain.Dto;
using Amver.Domain.Models;
using Amver.WebApi.CustomExceptionMiddleware;
using Amver.WebApi.Interfaces.Services;
using Amver.WebApi.Interfaces.Storages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using ValidationException = Amver.WebApi.CustomExceptionMiddleware.ValidationException;

namespace Amver.WebApi.Controllers
{
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IAccountService _accountService;
        private readonly IAuthStorage _authStorage;
        private readonly IStringLocalizer<AppResources> _stringLocalizer;

        public AuthController(IAuthService authService, IUserService userService, IAccountService accountService, IAuthStorage authStorage, IStringLocalizer<AppResources> stringLocalizer)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _authStorage = authStorage;
            _stringLocalizer = stringLocalizer;
        }

        [HttpGet("FacebookTokenLogin")]
        public async Task<ActionResult<TokenResponse>> FbLoginToken([FromQuery, Required] string marker)
        {
            if (!ModelState.IsValid) return BadRequest();

            return await _accountService.FbLogin(marker);
        }

        [HttpPost("ClassicTokenRegistration")]
        public async Task<ActionResult<TokenResponse>> ClassicTokenRegistration([FromBody, Required] UserCredential userCredential)
        {
            if (!ModelState.IsValid) return BadRequest();

            return await _accountService.CreateAnAccount(userCredential);
        }
        
        [HttpPost("ClassicTokenLogin")]
        public async Task<ActionResult<TokenResponse>> ClassicTokenLogin([FromBody, Required] UserCredential userCredential)
        {
            if (!ModelState.IsValid) return BadRequest();

            return await _accountService.ClassicLogin(userCredential);
        }
        
        [Authorize]
        [HttpPost("FinishRegistration")]
        public async Task<ActionResult<BaseResult>> FinishRegistration([FromBody, Required] UserDto userDto)
        {
            if (!ModelState.IsValid) return BadRequest();
            
            var login = HttpContext.User.Identity.Name;

            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            if (userAuth == null || userAuth.IsBanned)
                throw new ValidationException(_stringLocalizer["UserIsDeletedOrBanned"]);

            if (userAuth.ForceRelogin)
                throw new UnauthorizedException(string.Empty);

            userDto.Id = userAuth.UserId;

            return await _userService.UpdateUserAfterFinishedRegistrationAsync(userDto);
        }
        //выпилить со временем
        [Authorize]
        [HttpPost("getLogin")]
        public async Task<ActionResult<TokenResponse>> GetLogin([FromBody, Required] FcmTokenModel fcmTokenModel)
        {
            if (!ModelState.IsValid) return BadRequest();
            
            var login = HttpContext.User.Identity.Name;

            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            _authStorage.AddFcmToken(userAuth.UserId, fcmTokenModel.FcmToken, fcmTokenModel.Os);

            return new TokenResponse{Login = login};
        }

        [Authorize]
        [HttpPost("signOut")]
        public async Task SignOut([FromBody, Required] FcmTokenModel fcmTokenModel)
        {
            var login = HttpContext.User.Identity.Name;

            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            await _authStorage.SignOut(userAuth.UserId, fcmTokenModel.FcmToken);
        }
        
        [Authorize]
        [HttpPost("addUserFcmToken")]
        public async Task AddUserFcmToken([FromBody, Required] FcmTokenModel fcmTokenModel)
        {
            var login = HttpContext.User.Identity.Name;

            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            _authStorage.AddFcmToken(userAuth.UserId, fcmTokenModel.FcmToken, fcmTokenModel.Os);
        }
    }
}