using System.IO;
using Microsoft.Extensions.Configuration;

namespace Amver.EfCli
{
    public class Configurator
    {
        public static IConfigurationRoot GetConfiguration()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var builder = new ConfigurationBuilder()
                .SetBasePath(currentDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }
    }
}