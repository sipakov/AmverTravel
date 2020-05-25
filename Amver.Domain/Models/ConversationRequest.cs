using System;

namespace Amver.Domain.Models
{
    public class ConversationRequest
    {
        public Guid Guid { get; set; }

        public int TripId { get; set; }
    }
}