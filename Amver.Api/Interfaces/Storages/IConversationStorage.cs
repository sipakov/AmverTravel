using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amver.Domain.Entities;
using Amver.Domain.Models;

namespace Amver.Api.Interfaces.Storages
{
    public interface IConversationStorage
    {
        Task<Conversation> CreateAsync(Conversation conversation);
        Task<Conversation> GetAsync(int tripId, int userId);
        Task<IEnumerable<Conversation>> GetListAsync(int userId);
        Task<Conversation> GetByIdAsync(Guid conversationId);
        Task SetLastDateTimeConnectionUser(Guid conversationId, int userId);
        Task SetLastDateTimeConnectionTripHolder(Guid conversationId);
        Task<BaseResult> RemoveAsync(Guid conversationId);
    }
}