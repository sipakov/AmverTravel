using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FillCityAndCountryTables.Initialization;
using FillCityAndCountryTables.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FillCityAndCountryTables
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine(@"To start press any key");
            Console.ReadKey();
            Console.WriteLine(@"Start process");
            var sw = new Stopwatch();
            sw.Start();

            var aggregatorService = ServiceConfigurator.Initialize().GetService<IAggregatorService>();

            var fileName = Configurator.GetConfiguration().GetSection("FileName").Value;

            await aggregatorService.AggregateAsync("CityAndCountries.txt");

            sw.Stop();
            Console.WriteLine($@"Process finished success in {sw.Elapsed}");
            Console.ReadLine();
        }
    }
}
