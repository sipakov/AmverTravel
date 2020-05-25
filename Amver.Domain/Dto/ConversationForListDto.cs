using System;

namespace Amver.Domain.Dto
{
    public class ConversationForListDto
    {
        public Guid Id { get; set; }
        public int TripId { get; set; }
        public string FromCity { get; set; }
        public string FromCountry { get; set; }
        public string ToCity { get; set; }
        public string ToCountry { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
        public string UserLogin { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public byte[] Image { get; set; }
        public string LastMessage { get; set; }
        public DateTime? LastMessageDateTime { get; set; }
        public DateTime? UserLastConnectDateTme { get; set; }
        public DateTime? UserTripHolderLastConnectDateTme { get; set; }
        public string ImageUri { get; set; }
        public bool IsUnreadMessages { get; set; }
    }
}