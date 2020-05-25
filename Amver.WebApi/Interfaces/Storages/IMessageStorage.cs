using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amver.Domain.Entities;
using Amver.Domain.Models;

namespace Amver.WebApi.Interfaces.Storages
{
    public interface IMessageStorage
    {
        Task<Message> CreateAsync(Message message);
        
        Task<IEnumerable<Message>> GetByConversationIdAsync(Guid conversationId, int userId);
        Task<IEnumerable<Guid>> GetExistConversationIdsAsync(IEnumerable<Guid> conversationIds);
        IEnumerable<Message> GetLastMessagesByConversationIdAsync(IEnumerable<Guid> conversationIds);
    }
}