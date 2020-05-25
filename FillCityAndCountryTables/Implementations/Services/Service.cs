using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amver.Domain.Entities;
using FillCityAndCountryTables.Interfaces.Services;
using FillCityAndCountryTables.Interfaces.Storages;

namespace FillCityAndCountryTables.Implementations.Services
{
    public class Service : IService
    {
        private readonly IStorage _storage;

        public Service(IStorage storage)
        {
            _storage = storage;
        }

        public string GetFilePath(string fileName)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));

            var currentDirectory = Directory.GetCurrentDirectory();
            return currentDirectory + "/" + fileName;
        }

        private (string, string) GetCityAndCountryPairs(string line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));

            Regex rx = new Regex(@"(?<city>^([\w\W]+)(\,)*)\,\s{1}(?<country>.+?(?=\t))");

            Match m = rx.Match(line);
            var city = m.Groups["city"].Value;
            var country = m.Groups["country"].Value;

            if (string.IsNullOrEmpty(country))
                throw new ArgumentNullException(nameof(country));

            if (string.IsNullOrEmpty(city))
                throw new ArgumentNullException(nameof(city));

            if (m.Success)
            {
                city = city.Trim().ToLower();
                country = country.Trim().ToLower();
                city = char.ToUpper(city[0]) + city.Substring(1);
                country = char.ToUpper(country[0]) + country.Substring(1);

                return (country, city);
            }

            Console.WriteLine($"Parsing failed {m.Value}");
            throw new InvalidOperationException($"Parsing failed {m.Value}");
        }

        public SortedDictionary<string, SortedList<string, int>> GetCountryCityPairs(IEnumerable<string> linesFromFile)
        {
            if (linesFromFile == null) throw new ArgumentNullException(nameof(linesFromFile));

            var countryCityPairs = new SortedDictionary<string, SortedList<string, int>>();

            foreach (var line in linesFromFile)
            {
                (var country, var city) = GetCityAndCountryPairs(line);

                var sortedListForValue = new SortedList<string, int>
                {
                    { city, default }
                };

                var isSuccess = countryCityPairs.TryAdd(country, sortedListForValue);

                if (!isSuccess)
                {
                    var temp = countryCityPairs[country];
                    var isSuccessToSortedListForValue = temp.TryAdd(city, default);

                    if (!isSuccessToSortedListForValue)
                        throw new InvalidOperationException(nameof(isSuccessToSortedListForValue));                

                    countryCityPairs[country] = temp;
                }
            }
            return countryCityPairs;
        }
              
        public async Task InsertCountriesAndCitiesAsync(SortedDictionary<string, SortedList <string, int>> countryCityPairs)
        {
            if (countryCityPairs == null)         
                throw new ArgumentNullException(nameof(countryCityPairs));

            foreach (var countryCityPair in countryCityPairs)
            {
                var country = new Country { Name = countryCityPair.Key};

                var cities = countryCityPair.Value.Select(x=> new City { Name = x.Key}).ToList();

                var insertedCountry = await _storage.InsertCountryAsync(country, cities);

                await _storage.InsertCitiesAsync(insertedCountry.Id, cities);
            }
        }
    }
}
