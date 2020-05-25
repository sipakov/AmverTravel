using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amver.Domain.Entities;
using Amver.EfCli;
using Amver.WebApi.Interfaces.Storages;
using Microsoft.EntityFrameworkCore;

namespace Amver.WebApi.Implementations.Storages
{
    public class CityStorage : ICityStorage
    {
        private readonly IContextFactory<ApplicationContext> _contextFactory;

        public CityStorage(IContextFactory<ApplicationContext> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        public async Task<List<City>> GetCityListByNamePartAsNoTrackingAsync(string namePart, int countryId)
        {
            if (string.IsNullOrEmpty(namePart))
                throw new ArgumentException("Value cannot be null or empty.", nameof(namePart));

            await using var context = _contextFactory.CreateContext();
            var queryableTrip = context.Cities.Include(x=>x.Country).AsQueryable();
            var upperNamePart = namePart.First().ToString().ToUpper() + namePart.Substring(1);
            if (countryId > 0)
            {
                return await queryableTrip.Where(x =>
                        x.Name.StartsWith(upperNamePart)  && x.Country.Id == countryId)
                    .ToListAsync();  
            }
            return await queryableTrip.Where(x =>
                    x.Name.StartsWith(upperNamePart))
                .ToListAsync();
        }
        
        public async Task<List<City>> GetCityListByNamePartRuAsNoTrackingAsync(string namePart, int countryId)
        {
            if (string.IsNullOrEmpty(namePart))
                throw new ArgumentException("Value cannot be null or empty.", nameof(namePart));

            await using var context = _contextFactory.CreateContext();
            var queryableTrip = context.Cities.Include(x=>x.Country).AsQueryable();
            var upperNamePart = namePart.First().ToString().ToUpper() + namePart.Substring(1);
            if (countryId > 0)
            {
                return await queryableTrip.Where(x =>
                        x.ruRu.StartsWith(upperNamePart)  && x.Country.Id == countryId)
                    .ToListAsync();  
            }
            var result = await queryableTrip.Where(x =>
                    x.ruRu.StartsWith(upperNamePart)).Select(x=> new City{Name = x.ruRu, Id = x.Id, Country = new Country{Id = x.Country.Id, Name = x.Country.ruRu}})
                .ToListAsync();
            return result;
        }
    }
}