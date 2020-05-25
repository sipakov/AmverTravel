using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.Domain.Models;
using Amver.WebApi.CustomExceptionMiddleware;
using Amver.WebApi.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using ValidationException = Amver.WebApi.CustomExceptionMiddleware.ValidationException;

namespace Amver.WebApi.Controllers
{
    [Route("[controller]")]
    public class ConversationController : ControllerBase
    {
        private readonly IConversationService _conversationService;
        private readonly IAuthService _authService;
        private readonly IStringLocalizer<AppResources> _stringLocalizer;

        
        public ConversationController(IConversationService conversationService, IAuthService authService, IStringLocalizer<AppResources> stringLocalizer)
        {
            _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _stringLocalizer = stringLocalizer;
        }
        
        [Authorize]
        [HttpGet("getList")]
        public async Task<ActionResult<ConversationResponse>> GetListAsync()
        {
            if (!ModelState.IsValid) return BadRequest();
            
            var login = HttpContext.User.Identity.Name;
            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);
            if (userAuth == null)
                throw new ValidationException(_stringLocalizer["UserIsDeletedOrBanned"]);
            if (userAuth.ForceRelogin)
                throw new UnauthorizedException(string.Empty);

            return await _conversationService.GetListAsync(userAuth.UserId);
        }

        [Authorize]
        [HttpGet("remove")]
        public async Task<ActionResult<BaseResult>> RemoveAsync([FromQuery, Required] Guid conversationId)
        {
            if (!ModelState.IsValid) return BadRequest();

            var login = HttpContext.User.Identity.Name;

            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            if (userAuth == null)
                throw new ValidationException(_stringLocalizer["UserIsDeletedOrBanned"]);

            if (userAuth.ForceRelogin)
                throw new UnauthorizedException(string.Empty);

            return await _conversationService.RemoveAsync(conversationId);
        }
    }
}