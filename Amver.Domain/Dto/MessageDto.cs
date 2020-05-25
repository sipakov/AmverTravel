using System;
using Amver.Domain.Entities;

namespace Amver.Domain.Dto
{
    public class MessageDto
    {
        public long Id { get; set; }
        public int UserTripHolderId { get; set; }
        public User UserCompanion { get; set; }
        public int UserCompanionId { get; set; }
        public DateTime SendDate { get; set; }
        public DateTime SaveDate { get; set; }
        public int ConversationId { get; set; }
        
        public bool IsDelivered { get; set; }
        public bool IsFailed { get; set; }
        public bool IsDeleted { get; set; }
    }
}