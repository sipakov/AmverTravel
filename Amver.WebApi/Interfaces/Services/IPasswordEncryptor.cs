namespace Amver.WebApi.Interfaces.Services
{
    public interface IPasswordEncryptor
    {
        string Encrypt(string password);
    }
}