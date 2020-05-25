using System;
namespace Amver.Domain.Entities
{
    public class Message
    {
        public Guid ConversationId { get; set; }
        public DateTime SendDate { get; set; }
        public string MessageStr { get; set; }
        public DateTime SaveDate { get; set; }
        public int Sender { get; set; }
        public bool IsFailed { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsUnread { get; set; }
    }
}