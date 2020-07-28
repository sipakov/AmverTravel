using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amver.Api.Interfaces.City;
using Amver.Domain.Entities;
using Amver.EfCli;
using Microsoft.EntityFrameworkCore;

namespace Amver.Api.Implementations.City
{
    public class CityFromStorageGetter : ICityFromStorageGetter
    {
        private readonly IContextFactory<ApplicationContext> _contextFactory;

        public CityFromStorageGetter(IContextFactory<ApplicationContext> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        public async Task<IEnumerable<Domain.Entities.City>> GetListByNamePart(string namePart, int countryId, ApplicationContext context)
        {
            var targetContext = context ?? _contextFactory.CreateContext();
            var queryableTrip = targetContext.Cities.Include(x => x.Country).AsQueryable();
            var upperNamePart = namePart.First().ToString().ToUpper() + namePart.Substring(1);
            if (countryId > 0)
            {
                return await queryableTrip.Where(x =>
                        x.Name.StartsWith(upperNamePart) && x.Country.Id == countryId)
                    .ToListAsync();
            }

            return await queryableTrip.Where(x =>
                    x.Name.StartsWith(upperNamePart))
                .ToListAsync();
        }

        public async Task<IEnumerable<Domain.Entities.City>> GetListByNamePartRu(string namePart, int countryId, ApplicationContext context)
        {
            var targetContext = context ?? _contextFactory.CreateContext();
            var queryableTrip = targetContext.Cities.Include(x => x.Country).AsQueryable();
            var upperNamePart = namePart.First().ToString().ToUpper() + namePart.Substring(1);
            if (countryId > 0)
            {
                return await queryableTrip.Where(x =>
                        x.ruRu.StartsWith(upperNamePart) && x.Country.Id == countryId)
                    .ToListAsync();
            }

            var result = await queryableTrip.Where(x =>
                    x.ruRu.StartsWith(upperNamePart)).Select(x => new Domain.Entities.City
                    {Name = x.ruRu, Id = x.Id, Country = new Country {Id = x.Country.Id, Name = x.Country.ruRu}})
                .ToListAsync();
            return result;
        }
    }
}