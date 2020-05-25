using System;
using Microsoft.EntityFrameworkCore;

namespace Amver.EfCli
{
    public interface IContextFactory<out T> where T : DbContext
    {
        T CreateContext();

        T CreateContext(int timeOut);
    }
}
