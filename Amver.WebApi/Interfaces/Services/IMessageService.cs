using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Amver.Domain.Entities;
using Amver.Domain.Models;

namespace Amver.WebApi.Interfaces.Services
{
    public interface IMessageService
    {
        Task<Message> CreateAsync(SendModel sendModel, int userId, string message, Guid conversationId);
        Task<ObservableCollection<MessageModel>> GetByConversationIdAsync(Guid conversationId, int userId);
        Task MessageIsReadAsync(Message message);
        
    }
}