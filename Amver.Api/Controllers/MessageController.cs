using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Amver.Api.CustomExceptionMiddleware;
using Amver.Api.Interfaces.Services;
using Amver.Domain.Constants;
using Amver.Domain.Entities;
using Amver.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using ValidationException = Amver.Api.CustomExceptionMiddleware.ValidationException;

namespace Amver.Api.Controllers
{
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IAuthService _authService;
        private readonly IStringLocalizer<AppResources> _stringLocalizer;

        
        public MessageController(IMessageService messageService, IAuthService authService, IStringLocalizer<AppResources> stringLocalizer)
        {
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _stringLocalizer = stringLocalizer;
        }
        [Authorize]
        [HttpPost("getList")]
        public async Task<ActionResult<ObservableCollection<MessageModel>>> GetByConversationIdAsync([FromBody, Required] ConversationRequest conversationRequest)
        {
            if (!ModelState.IsValid) return BadRequest();
            
            var login = HttpContext.User.Identity.Name;

            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            if (userAuth == null || userAuth.IsBanned)
                throw new CustomExceptionMiddleware.ValidationException(_stringLocalizer["UserIsDeletedOrBanned"]);

            if (userAuth.ForceRelogin)
                throw new UnauthorizedException(string.Empty);
            
//            var trip = await _tripService.GetByIdAsync(conversationRequest.TripId);
//            if (trip != null && trip.UserId == userAuth.UserId)
//                throw new ValidationException(Messages.UserCannotWriteToYourself);

            return await _messageService.GetByConversationIdAsync(conversationRequest.Guid, userAuth.UserId);
        }
        
        [Authorize]
        [HttpPost("isRead")]
        public async Task MessageIsReadAsync([FromBody, Required] Message message)
        {
            var login = HttpContext.User.Identity.Name;

            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);

            if (userAuth == null || userAuth.IsBanned)
                throw new CustomExceptionMiddleware.ValidationException(_stringLocalizer["UserIsDeletedOrBanned"]);

            _ =  _messageService.MessageIsReadAsync(message);
        }
        
    }
}