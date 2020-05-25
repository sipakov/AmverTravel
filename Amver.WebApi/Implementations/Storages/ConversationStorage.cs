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
    public class ConversationStorage : IConversationStorage
    {
        private readonly IContextFactory<ApplicationContext> _contextFactory;

        public ConversationStorage(IContextFactory<ApplicationContext> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }
        
        public async Task<Conversation> CreateAsync(Conversation conversation)
        {
            if (conversation == null) throw new ArgumentNullException(nameof(conversation));

            using (var context = _contextFactory.CreateContext())
            {
                var newConversation = await context.Conversations.AddAsync(conversation);
                await context.SaveChangesAsync();

                return newConversation.Entity;
            }
        }

        public async Task<Conversation> GetAsync(int tripId, int userId)
        {
            if (tripId <= 0) throw new ArgumentOutOfRangeException(nameof(tripId));
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            
            using (var context = _contextFactory.CreateContext())
            {
                return await context.Conversations.AsNoTracking().FirstOrDefaultAsync(x => x.TripId == tripId && x.UserId == userId && !x.IsDeleted);
            }
        }

        public async Task<IEnumerable<Conversation>> GetListAsync(int userId)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            
            using (var context = _contextFactory.CreateContext())
            {
                var queryable = context.Conversations.Include(x=>x.User).Include(x => x.Trip).ThenInclude(x=>x.ToCountry).Include(x=>x.Trip).ThenInclude(x=>x.User);
                return await queryable.AsNoTracking().Where(x => (x.UserId == userId || x.Trip.User.Id == userId) && x.User.DeletedDate == null && !x.IsDeleted && !x.Trip.IsDeleted).ToListAsync();
            }
        }
        
        public async Task<Conversation> GetByIdAsync(Guid conversationId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                return await context.Conversations.AsNoTracking().FirstOrDefaultAsync(x=>x.Id == conversationId);
            }
        }
        
        public async Task SetLastDateTimeConnectionUser(Guid conversationId, int userId)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            using (var context = _contextFactory.CreateContext())
            {
                var targetConversation = await context.Conversations.AsNoTracking().FirstOrDefaultAsync(x=>x.Id == conversationId);

                if (targetConversation == null)
                        return;
                
                targetConversation.UserLastConnectDateTme = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }
        public async Task SetLastDateTimeConnectionTripHolder(Guid conversationId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                var targetConversation = await context.Conversations.AsNoTracking().FirstOrDefaultAsync(x=>x.Id == conversationId);

                if (targetConversation == null)
                    return;
                
                targetConversation.UserTripHolderLastConnectDateTme = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }
        }

        public async Task<BaseResult> RemoveAsync(Guid conversationId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                var entityToRemove = await context.Conversations.FirstOrDefaultAsync(x => x.Id == conversationId);
                if (entityToRemove == null)
                    throw new ArgumentNullException(nameof(Trip));
               
                    entityToRemove.IsDeleted = true;
                    await context.SaveChangesAsync();                                         
            }
            return new BaseResult();
        }
    }
}