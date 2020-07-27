using Amver.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Amver.Api.Tests
{
    public class SqlLiteDbContext : DbContext
    {
        public DbSet<Domain.Entities.City> Cities { get; set; }
        
        public DbSet<Domain.Entities.Country> Countries { get; set; }

        public SqlLiteDbContext(DbContextOptions<SqlLiteDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SqlLiteDbContext).Assembly);
            
            ConfigureCountriesModelCreation(modelBuilder);
            ConfigureCitiesModelCreation(modelBuilder);
        }
        
        private static void ConfigureCitiesModelCreation(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Domain.Entities.City>().HasKey(x => new { x.Id});
            modelBuilder.Entity<Domain.Entities.City>().HasIndex(x => x.CountryId);
            modelBuilder.Entity<Domain.Entities.City>().Property(x => x.ruRu).HasMaxLength(200);
        }
        
        private static void ConfigureCountriesModelCreation(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Country>().HasKey(x => new { x.Id});
            modelBuilder.Entity<Country>().Property(x => x.ruRu).HasMaxLength(200);
        }
    }
}