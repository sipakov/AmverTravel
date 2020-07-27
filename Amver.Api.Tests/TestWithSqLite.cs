using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Amver.Api.Tests
{
    public abstract class TestWithSqLite : IDisposable
    {
        private const string InMemoryConnectionString = "DataSource=:memory:";
        private readonly SqliteConnection _connection;

        protected readonly SqlLiteDbContext DbContext;

        protected TestWithSqLite()
        {
            _connection = new SqliteConnection(InMemoryConnectionString);
            _connection.Open();
            var options = new DbContextOptionsBuilder<SqlLiteDbContext>()
                .UseSqlite(_connection)
                .Options;
            DbContext = new SqlLiteDbContext(options);
            DbContext.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _connection.Close();
        }
    }
}