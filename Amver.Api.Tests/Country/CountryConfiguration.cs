using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amver.Api.Tests.City
{
    public class CountryConfiguration: IEntityTypeConfiguration<Domain.Entities.Country>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.Country> modelBuilder)
        {
            modelBuilder.HasKey(x => new { x.Id});
            modelBuilder.Property(x => x.ruRu).HasMaxLength(200);
            modelBuilder.Property(x => x.Name).HasMaxLength(200);
        }   
    }
}