using System;
using Microsoft.EntityFrameworkCore;

namespace Amver.EfCli
{
    public class ApplicationContextFactory : IContextFactory<ApplicationContext>
    {
        private readonly DbContextOptions<ApplicationContext> _appContextOptions;

        public ApplicationContextFactory(DbContextOptions<ApplicationContext> contextOptions = null)
        {
            _appContextOptions = contextOptions;
        }

        public ApplicationContext CreateContext()
        {
            return _appContextOptions == null ? new ApplicationContext() : new ApplicationContext(_appContextOptions);
        }

        public ApplicationContext CreateContext(int timeOut)
        {
            return _appContextOptions == null ? new ApplicationContext(timeOut) : new ApplicationContext(_appContextOptions);
        }
    }
}
