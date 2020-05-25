using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Amver.Domain.Dto;
using Amver.Domain.Models;
using Amver.WebApi.CustomExceptionMiddleware;
using Amver.WebApi.Interfaces;
using Amver.WebApi.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using ValidationException = Amver.WebApi.CustomExceptionMiddleware.ValidationException;

namespace Amver.WebApi.Controllers
{
    [Route("[controller]")]

    public class FavouriteTripController : ControllerBase
    {
        private readonly IFavouriteTripService _favouriteTripService;
        private readonly IAuthService _authService;
        private readonly IStringLocalizer<AppResources> _stringLocalizer;
        private readonly ICustomRequestCultureProvider _customRequestCultureProvider;


        
        public FavouriteTripController(IFavouriteTripService favouriteTripService, IAuthService authService, IStringLocalizer<AppResources> stringLocalizer, ICustomRequestCultureProvider customRequestCultureProvider)
        {
            _favouriteTripService = favouriteTripService ?? throw new ArgumentNullException(nameof(favouriteTripService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _stringLocalizer = stringLocalizer;
            _customRequestCultureProvider = customRequestCultureProvider;
        }
        
        [Authorize]
        [HttpPost("like")]
        public async Task<ActionResult<FavouriteTripDto>> LikeAsync([FromBody, Required] FavouriteTripDto favouriteTripDto)
        {
            if (!ModelState.IsValid) throw new ArgumentException(nameof(favouriteTripDto));
            //todo mb replace logic to local storage
            var login = HttpContext.User.Identity.Name;

            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            if (userAuth == null || userAuth.IsBanned)
                throw new ValidationException(_stringLocalizer["UserIsDeletedOrBanned"]);

            if (userAuth.ForceRelogin)
                throw new UnauthorizedException(string.Empty);
            
            favouriteTripDto.UserId = userAuth.UserId;

            return await _favouriteTripService.LikeAsync(favouriteTripDto);
        }
        
        [Authorize]
        [HttpGet("list")]
        public async Task<ActionResult<FavouriteTripResponse>> GetFavouriteTripListAsync()
        {
            if (!ModelState.IsValid) return BadRequest();
            var currentCulture = _customRequestCultureProvider.DetermineProviderCultureResult(HttpContext);

            var login = HttpContext.User.Identity.Name;

            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            if (userAuth.ForceRelogin)
                throw new UnauthorizedException(string.Empty);

            return await _favouriteTripService.GetFavouriteTripList(userAuth.UserId, currentCulture);
        }
        [Authorize]
        [HttpPost("get")]
        public async Task<ActionResult<FavouriteTripDto>> IsLikedTripAsync([FromBody, Required] FavouriteTripDto favouriteTripDto)
        {
            if (!ModelState.IsValid) return BadRequest();
            
            var login = HttpContext.User.Identity.Name;

            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            if (userAuth.ForceRelogin)
                throw new UnauthorizedException(string.Empty);

            favouriteTripDto.UserId = userAuth.UserId;

            return await _favouriteTripService.IsLikedTripAsync(favouriteTripDto);
        }
    }
}