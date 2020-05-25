using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Amver.EfCli.ServiceConfiguration
{
    public static class ServiceCollectionExtensions
    {
        public static void UseEfCli(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ApplicationContext>(options =>
                options.UseNpgsql(connectionString));
        }
    }
}