using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amver.Domain.Entities;
using Amver.EfCli;
using FillCityAndCountryTables.Interfaces.Storages;

namespace FillCityAndCountryTables.Implementations.Storages
{
    public class Storage : IStorage
    {
        private readonly IContextFactory<ApplicationContext> _context;

        public Storage(IContextFactory<ApplicationContext> context) 
        {
            _context = context;
        }
        public async Task<Country> InsertCountryAsync(Country country, IEnumerable<City> cities)
        {
            if (country == null)           
                throw new ArgumentNullException(nameof(country));

            if (cities == null)          
                throw new ArgumentNullException(nameof(cities));
                
            using (var appContext = _context.CreateContext())
            {
                try
                {
                    await appContext.Countries.AddAsync(country);

                    await appContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }
            }

            return country;
        }

        public async Task InsertCitiesAsync(int countryId, IEnumerable<City> cities) 
        {
            if (cities == null)         
                throw new ArgumentNullException(nameof(cities));

            if (countryId <= 0)
                throw new ArgumentOutOfRangeException(nameof(countryId));

            using (var appContext = _context.CreateContext())
            {
                try
                {
                    var citiesWithCountry = cities.Select(x => new City { Name = x.Name, CountryId = countryId }).ToList();
                                      
                        await appContext.AddRangeAsync(citiesWithCountry);
                        await appContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }
            }
        }
    }
}
