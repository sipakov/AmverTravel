using System;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Amver.Domain.Constants;
using Amver.Libraries.Validation.Interfaces;

namespace Amver.Libraries.Validation.Implementations
{
    public class Validation : IValidation
    {
        public MailAddress GetValidEmail(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return mailAddress;
            }
            catch
            {
                return null;
            }
        }
        
        public bool IsValidUserName(string userName)
        {
            if (string.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", nameof(userName));

            return Regex.IsMatch(userName, AuthOptions.PatternToValidateName);
        }
        
        public bool IsValidLogin(string login)
        {
            if (string.IsNullOrEmpty(login)) throw new ArgumentException("Value cannot be null or empty.", nameof(login));

            return Regex.IsMatch(login, AuthOptions.PatternToValidateLogin);
        }
    }
}