using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amver.Domain.Entities;

namespace FillCityAndCountryTables.Interfaces.Storages
{
    public interface IStorage
    {
        Task<Country> InsertCountryAsync(Country country, IEnumerable<City> cities);
        Task InsertCitiesAsync(int countryId, IEnumerable<City> cities);
    }
}
