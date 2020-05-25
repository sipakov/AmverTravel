using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amver.Domain.Entities;

namespace FillCityAndCountryTables.Interfaces.Services
{
    public interface IService
    {
        string GetFilePath(string fileName);

        SortedDictionary<string, SortedList<string, int>> GetCountryCityPairs(IEnumerable<string> linesFromFile);        

        Task InsertCountriesAndCitiesAsync(SortedDictionary<string, SortedList<string, int>> countryCityPairs);
    }
}
