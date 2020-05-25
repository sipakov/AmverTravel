using System;
using Amver.EfCli;
using FillCityAndCountryTables.Implementations.Services;
using FillCityAndCountryTables.Implementations.Storages;
using FillCityAndCountryTables.Interfaces.Services;
using FillCityAndCountryTables.Interfaces.Storages;
using Microsoft.Extensions.DependencyInjection;

namespace FillCityAndCountryTables.Initialization
{
    public class ServiceConfigurator
    {
        public static ServiceProvider Initialize()
        {
            var services = new ServiceCollection()
                .AddTransient<IStorage, Storage>()
                .AddTransient<IService, Service>()
                .AddTransient<IAggregatorService, AggregatorService>()
                .AddSingleton<IContextFactory<ApplicationContext>, ApplicationContextFactory>()
                .BuildServiceProvider();

            return services;
        }
    }
}
