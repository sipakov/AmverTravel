using System;
using Amver.EfCli;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Amver.Api.Tests
{
    public class ConnectionFactory : IDisposable, IContextFactory<ApplicationContext>
    {  
 
        #region IDisposable Support  
        private bool _disposedValue = false; // To detect redundant calls  

        public ApplicationContext CreateContext()
        {
            var connection = new SqliteConnection("DataSource=:memory:");  
            connection.Open();  
            var option = new DbContextOptionsBuilder<ApplicationContext>().UseSqlite(connection).Options;  
            
            var context = new ApplicationContext(option);

            context.Database.EnsureDeleted();  
            context.Database.EnsureCreated();

            return context;  
        }

        public ApplicationContext CreateContext(int timeOut)
        {
            var connection = new SqliteConnection("DataSource=:memory:");  
            connection.Open();  
  
            var option = new DbContextOptionsBuilder<ApplicationContext>().UseSqlite(connection).Options;  
  
            var context = new ApplicationContext(option);

            context.Database.EnsureDeleted();  
            context.Database.EnsureCreated();

            return context;  
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue) return;
            if (disposing)  
            {  
            }  
  
            _disposedValue = true;
        }  
  
        public void Dispose()  
        {  
            Dispose(true);  
        }  
        #endregion  
    }  
}