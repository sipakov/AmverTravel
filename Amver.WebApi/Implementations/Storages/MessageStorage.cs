using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amver.Domain.Entities;
using Amver.Domain.Models;
using Amver.EfCli;
using Amver.WebApi.Interfaces.Storages;
using Microsoft.EntityFrameworkCore;

namespace Amver.WebApi.Implementations.Storages
{
    public class MessageStorage : IMessageStorage
    {
        private readonly IContextFactory<ApplicationContext> _contextFactory;

        public MessageStorage(IContextFactory<ApplicationContext> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        public async Task<Message> CreateAsync(Message message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            
            var messageToResponse = new Message();
            using (var context = _contextFactory.CreateContext())
            {
                var newMessage =  await context.Messages.AddAsync(message);
                await context.SaveChangesAsync();
                messageToResponse = newMessage.Entity;
            }

            return messageToResponse;
        }

        public async Task<IEnumerable<Message>> GetByConversationIdAsync(Guid conversationId, int userId)
        {
            await using var context = _contextFactory.CreateContext();
            var messages = await context.Messages.Where(x => x.ConversationId == conversationId).ToListAsync();
            var messagesUnread = context.Messages.Where(x => x.ConversationId == conversationId && x.Sender !=userId  && x.IsUnread);
            foreach (var message in messagesUnread)
            {
                message.IsUnread = false;
            }
            await context.SaveChangesAsync();
            return messages;
        }
        
        public async Task<IEnumerable<Guid>> GetExistConversationIdsAsync(IEnumerable<Guid> conversationIds)
        {
            using (var context = _contextFactory.CreateContext())
            {
                var result = await context.Messages.AsNoTracking()
                    .Where(x => conversationIds.Contains(x.ConversationId)).GroupBy(x => x.ConversationId)
                    .Select(x => x.Key).ToListAsync();
                return result;
            }
        }
        
        public IEnumerable<Message> GetLastMessagesByConversationIdAsync(IEnumerable<Guid> conversationIds)
        {
            using (var context = _contextFactory.CreateContext())
            {
                //Todo use dapper. Not optimal
                var messages = context.Messages.AsNoTracking().Where(x => conversationIds.Contains(x.ConversationId))
                    .OrderBy(x => x.SaveDate).AsEnumerable().GroupBy(x => x.ConversationId).Select(x =>
                        new Message
                        {
                            ConversationId = x.Key, MessageStr = x.Last().MessageStr, SaveDate = x.Last().SaveDate, IsUnread = x.Last().IsUnread, Sender = x.Last().Sender
                        }).ToList();
                return messages;
            }
        }
    }
}