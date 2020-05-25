using Amver.EfCli;
using Microsoft.Extensions.DependencyInjection;

namespace FillObjectionableReasonTable.Initialization
{
    public class ServiceConfigurator
    {
        public static ServiceProvider Initialize()
        {
            var services = new ServiceCollection()
                .AddTransient<IFiller, Filler>()
                .AddSingleton<IContextFactory<ApplicationContext>, ApplicationContextFactory>()
                .BuildServiceProvider();

            return services;
        }
    }
}
