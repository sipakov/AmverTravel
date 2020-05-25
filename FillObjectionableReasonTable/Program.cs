using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FillObjectionableReasonTable.Initialization;
using Microsoft.Extensions.DependencyInjection;

namespace FillObjectionableReasonTable
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine(@"To start press any key");
            Console.ReadKey();
            Console.WriteLine(@"Start process");
            var sw = new Stopwatch();
            sw.Start();

            var filler = ServiceConfigurator.Initialize().GetService<IFiller>();

            await filler.Fill();

            sw.Stop();
            Console.WriteLine($@"Process finished success in {sw.Elapsed}");
            Console.ReadLine();
        } 
    }
}