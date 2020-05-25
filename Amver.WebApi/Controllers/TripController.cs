using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.Domain.Dto;
using Amver.Domain.Models;
using Amver.EfCli;
using Amver.WebApi.CustomExceptionMiddleware;
using Amver.WebApi.Interfaces;
using Amver.WebApi.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValidationException = Amver.WebApi.CustomExceptionMiddleware.ValidationException;

namespace Amver.WebApi.Controllers
{
    [Route("[controller]")]
    public class TripController : ControllerBase
    {
        private readonly ITripService _tripService;
        private readonly IAuthService _authService;
        private readonly IContextFactory<ApplicationContext> _contextFactory;
        private readonly ICustomRequestCultureProvider _customRequestCultureProvider;

        public TripController(ITripService tripService, IAuthService authService, IContextFactory<ApplicationContext> contextFactory, ICustomRequestCultureProvider customRequestCultureProvider)
        {
            _tripService = tripService ?? throw new ArgumentNullException(nameof(tripService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _customRequestCultureProvider = customRequestCultureProvider ?? throw new ArgumentNullException(nameof(customRequestCultureProvider));
        }
        
        [Authorize]
        [HttpPost("create")]
        public async Task<ActionResult<BaseResult>> CreateAsync([FromBody, Required] TripDto tripDto)
        {
            if (!ModelState.IsValid) return BadRequest();
            
            var login = HttpContext.User.Identity.Name;

            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            if (userAuth.ForceRelogin)
                throw new UnauthorizedException(string.Empty);

            tripDto.UserId = userAuth.UserId;
            return await _tripService.CreateAsync(tripDto);
        }

        [Authorize]
        [HttpGet("getMyTripList")]
        public async Task<ActionResult<TripResponse>> GetTripListByUserIdAsync()
        {
            if (!ModelState.IsValid) return BadRequest();
            var currentCulture = _customRequestCultureProvider.DetermineProviderCultureResult(HttpContext);

            var login = HttpContext.User.Identity.Name;

            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            if (userAuth.ForceRelogin)
                throw new UnauthorizedException(string.Empty);

            return await _tripService.GetListByUserIdAsync(userAuth.UserId, currentCulture);
        }

        [HttpPost("list")]
        public async Task<ActionResult<TripResponse>> GetListAsync(
            [FromBody, BindRequired] FilterTripRequest filterTripRequest)
        {
            if (!ModelState.IsValid) return BadRequest();
            var currentCulture = _customRequestCultureProvider.DetermineProviderCultureResult(HttpContext);

            return await _tripService.GetListAsync(filterTripRequest, currentCulture);
        }
        
        [Authorize]
        [HttpPost("authorizedList")]
        public async Task<ActionResult<TripResponse>> GetAuthorizedListAsync(
            [FromBody, BindRequired] FilterTripRequest filterTripRequest)
        {
            if (!ModelState.IsValid) return BadRequest();
            var currentCulture = _customRequestCultureProvider.DetermineProviderCultureResult(HttpContext);

            var login = HttpContext.User.Identity.Name;
            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);
            if (userAuth.ForceRelogin)
                throw new UnauthorizedException(string.Empty);
            return await _tripService.GetAuthorizedListAsync(filterTripRequest, userAuth.UserId, currentCulture);
        }

        [HttpGet("get")]
        public async Task<ActionResult<TripDto>> GetByIdAsync([FromQuery, Required] int tripId)
        {
            if (!ModelState.IsValid) return BadRequest();
            var currentCulture = _customRequestCultureProvider.DetermineProviderCultureResult(HttpContext);

            return await _tripService.GetByIdAsync(tripId, currentCulture);
        }
        
        [Authorize]
        [HttpGet("getAuthTrip")]
        public async Task<ActionResult<TripDto>> GetAuthByIdAsync([FromQuery, Required] int tripId)
        {
            if (!ModelState.IsValid) return BadRequest();
            var currentCulture = _customRequestCultureProvider.DetermineProviderCultureResult(HttpContext);

            var login = HttpContext.User.Identity.Name;
            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            await ValidateToBlock(userAuth.UserId, tripId);
            
            return await _tripService.GetByIdAsync(tripId, currentCulture);
        }
        
        [Authorize]
        [HttpPost("update")]
        public async Task<ActionResult<BaseResult>> UpdateAsync([FromBody, BindRequired] TripDto tripDto)
        {
            if (!ModelState.IsValid) return BadRequest();
            
            var login = HttpContext.User.Identity.Name;
            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);
            if (userAuth.ForceRelogin)
                throw new UnauthorizedException(string.Empty);

            tripDto.UserId = userAuth.UserId;
            return await _tripService.UpdateAsync(tripDto);
        }

        [Authorize]
        [HttpGet("getMyTrip")]
        public async Task<ActionResult<TripDto>> GetMyTripByIdAsync([FromQuery, Required] int tripId)
        {
            if (!ModelState.IsValid) return BadRequest();
            var currentCulture = _customRequestCultureProvider.DetermineProviderCultureResult(HttpContext);

            var login = HttpContext.User.Identity.Name;

            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            if (userAuth.ForceRelogin)
                throw new UnauthorizedException(string.Empty);

            var trip = await _tripService.GetByIdAsync(tripId, currentCulture);
            if (trip.UserId != userAuth.UserId)
                throw new UnauthorizedException("The trip does not belong to the user");
            
            return trip;
        }
        
        [Authorize]
        [HttpGet("remove")]
        public async Task<ActionResult<BaseResult>> RemoveTripByIdAsync([FromQuery, Required] int tripId)
        {
            if (!ModelState.IsValid) return BadRequest();
            var currentCulture = _customRequestCultureProvider.DetermineProviderCultureResult(HttpContext);

            var login = HttpContext.User.Identity.Name;

            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            if (userAuth.ForceRelogin)
                throw new UnauthorizedException(string.Empty);

            var trip = await _tripService.GetByIdAsync(tripId, currentCulture);
            if (trip.UserId != userAuth.UserId)
                throw new UnauthorizedException("The trip does not belong to the user");

            return await _tripService.RemoveAsync(tripId);
        }
        
        [Authorize]
        [HttpGet("complete")]
        public async Task<ActionResult<BaseResult>> CompleteTripByIdAsync([FromQuery, Required] int tripId)
        {
            if (!ModelState.IsValid) return BadRequest();
            var currentCulture = _customRequestCultureProvider.DetermineProviderCultureResult(HttpContext);

            var login = HttpContext.User.Identity.Name;

            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            if (userAuth.ForceRelogin)
                throw new UnauthorizedException(string.Empty);

            var trip = await _tripService.GetByIdAsync(tripId, currentCulture);
            if (trip.UserId != userAuth.UserId)
                throw new UnauthorizedException("The trip does not belong to the user");
            
            return await _tripService.CompleteAsync(tripId);
        }
        
        private async Task ValidateToBlock(int myUserId, int tripId)
        {
            if (myUserId <= 0) throw new ArgumentOutOfRangeException(nameof(myUserId));
            if (tripId <= 0) throw new ArgumentOutOfRangeException(nameof(tripId));

            using (var context = _contextFactory.CreateContext())
            {
                var targetTrip = await context.Trips.FirstOrDefaultAsync(x => x.Id == tripId);
                if (targetTrip!=null)
                {
                    var targetUserId = targetTrip.UserId;
                    var userToBlockedUser = await context.UserToBlockedUsers.FindAsync(targetUserId, myUserId);
                    if (userToBlockedUser != null)
                    {
                        throw new ValidationException("You blocked");  
                    }   
                }
               
            }
        }
    }
}