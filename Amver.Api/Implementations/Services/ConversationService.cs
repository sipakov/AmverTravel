using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amver.Api.Interfaces.Services;
using Amver.Api.Interfaces.Storages;
using Amver.Domain.Entities;
using Amver.Domain.Dto;
using Amver.Domain.Models;
using Microsoft.AspNetCore.Hosting;

namespace Amver.Api.Implementations.Services
{
    public class ConversationService : IConversationService
    {
        private readonly IConversationStorage _conversationStorage;
        private readonly IHostingEnvironment _appEnvironment;
        private readonly IMessageStorage _messageStorage;

        public ConversationService(IConversationStorage conversationStorage, IHostingEnvironment appEnvironment, IMessageStorage messageStorage)
        {
            _conversationStorage = conversationStorage ?? throw new ArgumentNullException(nameof(conversationStorage));
            _appEnvironment = appEnvironment ?? throw new ArgumentNullException(nameof(appEnvironment));
            _messageStorage = messageStorage ?? throw new ArgumentNullException(nameof(messageStorage));
        }

        public async Task<Conversation> CreateAsync(int tripId, int userId)
        {
            if (tripId <= 0) throw new ArgumentOutOfRangeException(nameof(tripId));
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            var id = Guid.NewGuid();

            var conversation = new Conversation
            {
                Id = id,
                TripId = tripId,
                UserId = userId,
                CreatedDate = DateTime.UtcNow
            };
            var newConversation = await _conversationStorage.CreateAsync(conversation);
            return newConversation;
        }

        public async Task<ConversationDto> GetAsync(int tripId, int userId)
        {
            if (tripId <= 0) throw new ArgumentOutOfRangeException(nameof(tripId));
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            var conversation = await _conversationStorage.GetAsync(tripId, userId);
            
            if (conversation == null) return null;
            
            var conversationDto = new ConversationDto
            {
                Id = conversation.Id,
                TripId = conversation.TripId,
                UserId = conversation.UserId,
                CreatedDate = conversation.CreatedDate,
                IsDeleted = conversation.IsDeleted
            };
            return conversationDto;
        }
        
        public async Task<ConversationResponse> GetListAsync(int userId)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            
            var conversations = (await _conversationStorage.GetListAsync(userId)).ToList();

            if (!conversations.Any())
            {
                return new ConversationResponse
                {
                    Conversations = new List<ConversationForListDto>(),
                    Count = default
                }; 
            }
            var conversationIds = conversations.Select(x => x.Id).ToList();
            var existConversationIdsList = await _messageStorage.GetExistConversationIdsAsync(conversationIds);
            var targetConversationIds = conversationIds.Intersect(existConversationIdsList);
            var targetConversations = conversations.Where(x => targetConversationIds.Contains(x.Id));
            var pathToDirectory = Path.Combine(_appEnvironment.WebRootPath, "Photos", "Users");
            var lastMessages = _messageStorage.GetLastMessagesByConversationIdAsync(targetConversationIds).ToList();
            
            var conversationDtoList = new List<ConversationForListDto>();

            foreach (var x in targetConversations)
            {
                var targetUser = x.UserId != userId ? x. User : x.Trip.User;
                var conversationId = x.Id;
                var lastMessage = lastMessages.FirstOrDefault(y => y.ConversationId == conversationId);
                int lastMessageContentLength = default;
                const int maxLength = 25;
                if (lastMessage != null)
                    lastMessageContentLength = lastMessage.MessageStr.Length;

                var targetPath = Path.Combine(pathToDirectory, $"{targetUser.Login}");
                var targetImages = new List<string>();
                if (Directory.Exists(targetPath))
                {
                    targetImages = Directory.GetFiles(targetPath, "*_conversation.jpeg").ToList();
                }
                
                var conversationDto = new ConversationForListDto
                {
                    Id = x.Id,
                    TripId = x.TripId,
                    UserId = targetUser.Id,
                    UserName = targetUser.FirstName,
                    ToCountry = x.Trip.ToCountry.Name,
                    DateFrom = x.Trip.DateFrom,
                    DateTo = x.Trip.DateTo,
                    CreatedDate = x.CreatedDate,
                    IsDeleted = x.IsDeleted,
                    LastMessage = lastMessageContentLength > maxLength ? lastMessage?.MessageStr.Substring(0, maxLength) : lastMessage?.MessageStr,
                    LastMessageDateTime = lastMessage?.SaveDate,
                    UserLastConnectDateTme = x.UserLastConnectDateTme,
                    UserTripHolderLastConnectDateTme = x.UserTripHolderLastConnectDateTme,
                    ImageUri = targetImages.Any() ? $"https://www.amver.net/Photos/Users/{targetUser.Login}/{Path.GetFileName(targetImages.First())}" : Path.Combine("https://www.amver.net", "images", "userAccountIcon.png"),
                    IsUnreadMessages = lastMessage != null && lastMessage.IsUnread && lastMessage.Sender != userId
                };
                conversationDtoList.Add(conversationDto);
            }

            var orderedByLastMessageDateConversationList =
                conversationDtoList.OrderByDescending(x => x.LastMessageDateTime).ToList();
            var count = orderedByLastMessageDateConversationList.Count;
            var conversationResponse = new ConversationResponse
            {
                Conversations = orderedByLastMessageDateConversationList,
                Count = count
            };
            
            return conversationResponse;
        }

        public async Task<ConversationDto> GetById(Guid conversationId)
        {
            var conversation = await _conversationStorage.GetByIdAsync(conversationId);
            
            if (conversation == null) return null;
            
            var conversationDto = new ConversationDto
            {
                Id = conversation.Id,
                TripId = conversation.TripId,
                UserId = conversation.UserId,
                CreatedDate = conversation.CreatedDate,
                IsDeleted = conversation.IsDeleted
            };
            return conversationDto;
        }

        public async Task SetLastDateTimeConnectionUser(ConversationDto conversationDto, int userId)
        {
            if (conversationDto == null) throw new ArgumentNullException(nameof(conversationDto));
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            if (conversationDto.UserId == userId)
            {
                await _conversationStorage.SetLastDateTimeConnectionUser(conversationDto.Id, userId);
            }
            else
            {
                await _conversationStorage.SetLastDateTimeConnectionTripHolder(conversationDto.Id);
            }
        }

        public async Task<BaseResult> RemoveAsync(Guid conversationId)
        {
            return await _conversationStorage.RemoveAsync(conversationId);
        }
    }
}