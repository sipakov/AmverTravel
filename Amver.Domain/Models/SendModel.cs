using System;

namespace Amver.Domain.Models
{
    public class SendModel
    {
        public int TripId { get; set; }
        public int UserToId { get; set; }
        public DateTime SendDate { get; set; }
        public Guid ConversationId { get; set; }
    }
}