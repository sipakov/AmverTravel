using System;

namespace Amver.Domain.Entities
{
    public class UserToBlockedUser
    {
        public int UserId { get; set; }
        
        public virtual User User { get; set; }

        public int BlockedUserId { get; set; }
        
        public virtual User BlockedUser { get; set; }

        public DateTime BlockingDate { get; set; }
    }
}