using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amver.Domain.Dto;
using Amver.Domain.Entities;
using Amver.Domain.Models;

namespace Amver.Api.Interfaces.Services
{
    public interface IConversationService
    {
        Task<Conversation> CreateAsync(int tripId, int userId);
        Task<ConversationDto> GetAsync(int tripId, int userId);
        Task<ConversationResponse> GetListAsync(int userId);
        Task<ConversationDto> GetById(Guid conversationId);
        Task SetLastDateTimeConnectionUser(ConversationDto conversationDto, int userId);
        Task<BaseResult> RemoveAsync(Guid conversationId);
    }
}