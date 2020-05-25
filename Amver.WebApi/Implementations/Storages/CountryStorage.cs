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
    public class CountryStorage : ICountryStorage
    {
        private readonly IContextFactory<ApplicationContext> _contextFactory;

        public CountryStorage(IContextFactory<ApplicationContext> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        public async Task<List<Country>> GetCountryListByNamePartAsNoTrackingAsync(string namePart)
        {
            if (string.IsNullOrEmpty(namePart))
                throw new ArgumentException("Value cannot be null or empty.", nameof(namePart));

            await using var context = _contextFactory.CreateContext();
            var upperNamePart = namePart.First().ToString().ToUpper() + namePart.Substring(1);
            return await context.Countries.AsNoTracking().Where(x =>
                    x.Name.StartsWith(upperNamePart))
                .ToListAsync();
        }
        
        public async Task<List<Country>> GetCountryListByNamePartRuAsNoTrackingAsync(string namePart)
        {
            if (string.IsNullOrEmpty(namePart))
                throw new ArgumentException("Value cannot be null or empty.", nameof(namePart));

            await using var context = _contextFactory.CreateContext();
            var upperNamePart = namePart.First().ToString().ToUpper() + namePart.Substring(1);
            var result = await context.Countries.AsNoTracking().Where(x =>
                    x.ruRu.StartsWith(upperNamePart)).Select(x=> new Country{Name = x.ruRu, Id = x.Id})
                .ToListAsync();
            return result;
        }
    }
}