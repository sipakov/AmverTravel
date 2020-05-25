using System.Threading.Tasks;
using Amver.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Amver.WebApi.Interfaces.Services
{
    public interface IProfileService
    {
        BaseResult RemoveProfileImage(string login);
    }
}