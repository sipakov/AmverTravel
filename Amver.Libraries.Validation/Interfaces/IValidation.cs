using System.Net.Mail;

namespace Amver.Libraries.Validation.Interfaces
{
    public interface IValidation
    {
        MailAddress GetValidEmail(string email);
        bool IsValidUserName(string login);
        bool IsValidLogin(string login);
    }
}