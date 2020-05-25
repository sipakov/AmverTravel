using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.Domain.Dto;
using Amver.Domain.Entities;
using Amver.WebApi.Interfaces.Services;
using Amver.WebApi.Interfaces.Storages;

namespace Amver.WebApi.Implementations.Services
{
    public class CountryService : ICountryService
    {
        private readonly ICountryStorage _countryStorage;

        public CountryService(ICountryStorage countryStorage)
        {
            _countryStorage = countryStorage ?? throw new ArgumentNullException(nameof(countryStorage));
        }

        public async Task<IEnumerable<CountryDto>> GetCountryListByNamePartAsync(NamePartDto namePartDto, string currentCulture)
        {
            if (namePartDto == null) throw new ArgumentNullException(nameof(namePartDto));
            if (string.IsNullOrEmpty(namePartDto.Part)) throw new ArgumentNullException(nameof(namePartDto.Part));
            
            var namePart = namePartDto.Part;
            
            List<Country> countryList;

            switch (currentCulture)
            {
                case Cultures.En: 
                    countryList = await _countryStorage.GetCountryListByNamePartAsNoTrackingAsync(namePart);
                    break;
                case Cultures.Ru: 
                    countryList = await _countryStorage.GetCountryListByNamePartRuAsNoTrackingAsync(namePart);
                    break;
                default:
                    countryList = await _countryStorage.GetCountryListByNamePartAsNoTrackingAsync(namePart);
                    break;
            }
            
            var countryDtoList = countryList.Select(x => new CountryDto
            {
                Id = x.Id,
                Name = x.Name
            });

            return countryDtoList;
        }
    }
}