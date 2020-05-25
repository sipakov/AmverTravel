using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.Domain.Dto;
using Amver.Domain.Models;
using Amver.EfCli;
using Amver.WebApi.CustomExceptionMiddleware;
using Amver.WebApi.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using ValidationException = Amver.WebApi.CustomExceptionMiddleware.ValidationException;

namespace Amver.WebApi.RealTimeCommunication.SignalR
{
    public class ChatHub : Hub
    {
        private readonly IMessageService _messageService;
        private readonly IConversationService _conversationService;
        private readonly IAuthService _authService;
        private readonly IContextFactory<ApplicationContext> _contextFactory;
        private readonly IStringLocalizer<AppResources> _stringLocalizer;


        public ChatHub(IMessageService messageService, IConversationService conversationService,
            IAuthService authService, IContextFactory<ApplicationContext> contextFactory, IStringLocalizer<AppResources> stringLocalizer)
        {
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
            _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _contextFactory = contextFactory;
            _stringLocalizer = stringLocalizer;
        }

        [Authorize]
        public async Task SendMessage(SendModel sendModel, string message)
        {
            if (sendModel == null) throw new ArgumentNullException(nameof(sendModel));
            var login = Context.User.Identity.Name;
            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);
            if (userAuth == null)
                throw new ValidationException(_stringLocalizer["UserIsDeletedOrBanned"]);
            
            var conversation = await _conversationService.GetById(sendModel.ConversationId);
            
            var createdMessage = await _messageService.CreateAsync(sendModel, userAuth.UserId, message, conversation.Id);
            
            await Clients.Group(conversation.Id.ToString()).SendAsync("ReceiveMessage", userAuth.UserId.ToString(), createdMessage);
          
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var fcmToken = Configurator.GetConfiguration().GetSection("FcmToken").Value;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", fcmToken);

            const int maxLength = 70;
            await using var context = _contextFactory.CreateContext();
            int targetUser;
            if(conversation.UserId == userAuth.UserId)
            {
                targetUser = context.Trips.First(x => x.Id == sendModel.TripId).UserId;
            }
            else
            {
                targetUser = conversation.UserId;
            }
                
            var userToFcmTokens = context.UserToFcmTokens.Where(x=>x.UserId == targetUser && x.IsInApp && DateTime.UtcNow <= x.LastSignIn + TimeSpan.FromMinutes(AuthOptions.LifeTimeMinutes)).ToList();
            if (!userToFcmTokens.Any())
            {
                return;
            }
            var targetTokens = new List<string>();
            targetTokens.AddRange(userToFcmTokens.Select(x=>x.FcmToken).ToList());
            var push = new Push
            {
                registration_ids = targetTokens,
                notification = new Notification
                {
                    title = userAuth.UserName,
                    body = message.Length > maxLength ? message.Substring(0, maxLength) : message,
                    content_available = true,
                    priority = "high",
                    badge = 1,
                    sound = "default",
                    icon = "ic_launcher_notification"
                },
                data = new Data
                {
                    conversation_id = conversation.Id.ToString()                   
                }
            };
            var content = JsonConvert.SerializeObject(push);
            HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");
            const string url = "https://fcm.googleapis.com/fcm/send";
            var result = await httpClient.PostAsync(url, httpContent);
        }

        [Authorize]
        public async Task JoinChat(int tripId, Guid conversationId)
        {
            var login = Context.User.Identity.Name;
            var userAuth = await _authService.GetActiveUserByLoginAsNoTrackingAsync(login);
            if (userAuth == null)
                throw new ValidationException(_stringLocalizer["UserIsDeletedOrBanned"]);
            if (userAuth.ForceRelogin)
                throw new UnauthorizedException(string.Empty);
            
            ConversationDto conversation;
            if (conversationId != Guid.Empty)
            {
                conversation = await _conversationService.GetById(conversationId);
            }
            else
            {
                conversation = await _conversationService.GetAsync(tripId, userAuth.UserId);   
            }
            
            if (conversation == null)
            {
                var newConversation = await _conversationService.CreateAsync(tripId, userAuth.UserId);
                await Groups.AddToGroupAsync(Context.ConnectionId, newConversation.Id.ToString());
                await Clients.Group(newConversation.Id.ToString()).SendAsync("JoinChat", newConversation.Id);
            }
            else
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, conversation.Id.ToString());
                await Clients.Group(conversation.Id.ToString()).SendAsync("JoinChat", conversation.Id);
                await _conversationService.SetLastDateTimeConnectionUser(conversation, userAuth.UserId);
            }
        }

        public async Task LeaveChat(string user)
        {
            await Clients.All.SendAsync("LeaveChat", user);
        }
    }
}