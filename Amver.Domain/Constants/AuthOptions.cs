using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Amver.Domain.Constants
{
    public static class AuthOptions
    {
        public const string Issuer = "AmverAuthServer";
        public const string Audience = "http://localhost:5000/";
        private const string Key = "amversecret_secretkey";
        public static SymmetricSecurityKey SymmetricSecurityKey { get; set; } = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key));
        public const int LifeTimeMinutes = 43800;
        public const string PatternToValidatePassword = @"^[a-zA-Z0-9@#$%&*+\-_(),+':;?.,!\[\]\s\\/]+$";
        public const string PatternToValidateLogin = @"^[a-zA-Z0-9@#$%&*+\-_(),+':;?.,!\[\]\s\\/]+$";
        public const string PatternToValidateName = @"^[a-zA-ZА-Яа-я]{1,20}";
        public const string AccessTokenAmver = "accessTokenAmver";
        public const string FcmTokenAmver = "fcmTokenAmver";
    }
}