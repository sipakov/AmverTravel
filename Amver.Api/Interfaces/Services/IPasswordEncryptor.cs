namespace Amver.Api.Interfaces.Services
{
    public interface IPasswordEncryptor
    {
        string Encrypt(string password);
    }
}