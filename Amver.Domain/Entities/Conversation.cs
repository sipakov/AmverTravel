using System;
namespace Amver.Domain.Entities
{
    public class Conversation
    {
        public Guid Id { get; set; }
        public virtual Trip Trip { get; set; }
        public int TripId { get; set; }
        public virtual User User { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? UserLastConnectDateTme { get; set; }
        public DateTime? UserTripHolderLastConnectDateTme { get; set; }
        
        public override int GetHashCode() {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj) {
            return Equals(obj as Conversation);
        }

        private bool Equals(Conversation obj) {
            return obj != null && obj.TripId == TripId && obj.UserId == UserId;
        }
    }
}
