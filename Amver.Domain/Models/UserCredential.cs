using System;
using Newtonsoft.Json;

namespace Amver.Domain.Models
{
    public class UserCredential
    {
        [JsonRequired]
        public string Login { get; set; }
        [JsonRequired]
        public string UserPassword { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime UserBirthDay { get; set; }

        public bool IsRegistration { get; set; }

        public string FcmToken { get; set; }

        public int Os { get; set; }
    }
}