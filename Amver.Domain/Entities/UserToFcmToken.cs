using System;

namespace Amver.Domain.Entities
{
    public class UserToFcmToken
    {
        public int UserId { get; set; }

        public virtual User User { get; set; }

        public string FcmToken { get; set; }

        public DateTime LastSignIn { get; set; }

        public DateTime LastVisit { get; set; }

        public bool IsInApp { get; set; }

        public int? Os { get; set; }
    }
}