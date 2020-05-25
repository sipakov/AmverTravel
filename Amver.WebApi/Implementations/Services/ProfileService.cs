using System;
using System.IO;
using Amver.Domain.Constants;
using Amver.Domain.Models;
using Amver.WebApi.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;

namespace Amver.WebApi.Implementations.Services
{
    public class ProfileService :  IProfileService
    {
        private readonly IHostingEnvironment _appEnvironment;

        public ProfileService(IHostingEnvironment appEnvironment)
        {
            _appEnvironment = appEnvironment ?? throw new ArgumentNullException(nameof(appEnvironment));
        }

        public BaseResult RemoveProfileImage(string login)
        {
            var pathToDirectory = Path.Combine(_appEnvironment.WebRootPath, "Photos", "Users", $"{login}");

            var di = new DirectoryInfo(pathToDirectory);

            foreach (var file in di.GetFiles())
            {
                file.Delete(); 
            }
            
            return new BaseResult();
        }
    }
}