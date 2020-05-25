using System;

namespace Amver.Domain.Dto
{
    public class ConversationDto
    {
        public Guid Id { get; set; }
        public int TripId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}