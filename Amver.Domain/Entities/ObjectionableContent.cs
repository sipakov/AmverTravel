using System;

namespace Amver.Domain.Entities
{
    public class ObjectionableContent
    {
        public int Id { get; set; }

        public int? UserId { get; set; }
        
        public virtual User ObjectionableUser { get; set; }

        public int ObjectionableUserId { get; set; }
        
        public int ObjectionableReasonId { get; set; }

        public int? TripId { get; set; }

        public string Comment { get; set; }

        public DateTime BanDate { get; set; }
    }
}