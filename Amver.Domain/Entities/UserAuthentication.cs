using System;

namespace Amver.Domain.Entities
{
    public class UserAuthentication
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public string HashedPassword { get; set; }

        public DateTime LastFailedSingInAttempt { get; set; }

        public int FailedSignInAttemptsCount { get; set; }

        public bool ForceRelogin { get; set; }

        public bool IsEmailConfirmed { get; set; }
    }
}