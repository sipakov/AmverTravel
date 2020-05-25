using Amver.EfCli;
using Microsoft.Extensions.DependencyInjection;

namespace LocalizationCityAndCountryTable
{
    public class ServiceConfigurator
    {
        public static ServiceProvider Initialize()
        {
            var services = new ServiceCollection()
                .AddTransient<ILocalizationService, LocalizationService>()
                .AddSingleton<IContextFactory<ApplicationContext>, ApplicationContextFactory>()
                .BuildServiceProvider();

            return services;
        }
    }
}
