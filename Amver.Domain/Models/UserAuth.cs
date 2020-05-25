using System;

namespace Amver.Domain.Models
{
    public class UserAuth
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public bool ForceRelogin { get; set; }
        public string Email { get; set; }
        public int? Gender { get; set; }

        public bool IsBanned { get; set; }
    }
}