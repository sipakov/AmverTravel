using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace LocalizationCityAndCountryTable
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
            var service = ServiceConfigurator.Initialize().GetService<ILocalizationService>();
            var result = await service.FillTargetColumn();

            sw.Stop();
            Console.WriteLine($@"Process finished success in {sw.Elapsed}");
            Console.ReadLine();
        }
    }
}