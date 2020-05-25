using System;
using System.IO;
using System.Threading.Tasks;
using FillCityAndCountryTables.Interfaces.Services;

namespace FillCityAndCountryTables.Implementations.Services
{
    public class AggregatorService : IAggregatorService
    {
        private readonly IService _service;

        public AggregatorService(IService service) 
        {
            _service = service;
        }

        public async Task AggregateAsync(string fileName)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));

            var filePath = _service.GetFilePath(fileName);

            var linesFromFile = File.ReadAllLines(filePath);

            //ToDo implement with bulk insert

            var countryCityPairs = _service.GetCountryCityPairs(linesFromFile);

            await _service.InsertCountriesAndCitiesAsync(countryCityPairs);
        }
    }
}
