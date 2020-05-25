using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Amver.Domain.Entities;
using Amver.Domain.Models;
using Amver.EfCli;
using Amver.WebApi.Interfaces.Services;
using Amver.WebApi.Interfaces.Storages;

namespace Amver.WebApi.Implementations.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageStorage _messageStorage;
        private readonly IContextFactory<ApplicationContext> _contextFactory;
        
        public MessageService(IMessageStorage messageStorage, IContextFactory<ApplicationContext> contextFactory)
        {
            _messageStorage = messageStorage ?? throw new ArgumentNullException(nameof(messageStorage));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        public async Task<Message> CreateAsync(SendModel sendModel, int userId, string message, Guid conversationId)
        {
            if (sendModel == null) throw new ArgumentNullException(nameof(sendModel));
            if (string.IsNullOrEmpty(message)) throw new ArgumentException("Value cannot be null or empty.", nameof(message));
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            var newMessage = new Message
            {
                SendDate = sendModel.SendDate,
                SaveDate = DateTime.UtcNow,
                ConversationId = conversationId,
                MessageStr = message,
                Sender = userId,
                IsUnread = true
            };
            var createdMessage = await _messageStorage.CreateAsync(newMessage);
            return createdMessage;
        }

        public async Task<ObservableCollection<MessageModel>> GetByConversationIdAsync(Guid conversationId, int userId)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            
            var messages = await _messageStorage.GetByConversationIdAsync(conversationId, userId);
            var orderedMessages = messages.OrderBy(x => x.SendDate);
            var messageList = new ObservableCollection<MessageModel>();
            foreach (var message in orderedMessages)
            {
                var convertedMessage = new MessageModel
                {
                    Message = message.MessageStr,
                    IsOwnMessage = message.Sender == userId,
                    UserId = message.Sender
                };
                messageList.Add(convertedMessage);
            }
            
            return messageList;
        }
        
        public async Task MessageIsReadAsync(Message message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            await using var context = _contextFactory.CreateContext();
            var targetMessage = await context.Messages.FindAsync(message.SaveDate, message.Sender, message.ConversationId);
            if (targetMessage != null)
            {
                targetMessage.IsUnread = false;
            }

            await context.SaveChangesAsync();
        }

    }
}