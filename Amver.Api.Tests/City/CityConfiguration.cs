using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Amver.Api.Tests.City
{
    public class EntityConfiguration: IEntityTypeConfiguration<Domain.Entities.City>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.City> modelBuilder)
        {
            modelBuilder.HasKey(x => new { x.Id});
            modelBuilder.HasIndex(x => x.CountryId);
            modelBuilder.Property(x => x.ruRu).HasMaxLength(200);
            modelBuilder.Property(x => x.Name).HasMaxLength(200);
        }   
    }
}