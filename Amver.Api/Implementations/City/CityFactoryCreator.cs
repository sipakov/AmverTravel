using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amver.Api.Interfaces.City;
using Amver.Domain.Constants;
using Amver.Domain.Dto;

namespace Amver.Api.Implementations.City
{
    
    public class CityFactoryCreator : ICityFactoryCreator
    {
        private readonly ICityFromStorageGetter _cityFromStorageGetter;

        public CityFactoryCreator(ICityFromStorageGetter cityFromStorageGetter)
        {
            _cityFromStorageGetter = cityFromStorageGetter ?? throw new ArgumentNullException(nameof(cityFromStorageGetter));
        }

        public async Task<IEnumerable<CityDto>> GetCityListByNamePart(NamePartDto namePartDto, string currentCulture)
        {
            if (namePartDto == null) throw new ArgumentNullException(nameof(namePartDto));
            if (string.IsNullOrEmpty(namePartDto.Part)) throw new ArgumentNullException(nameof(namePartDto.Part));
            
            var namePart = namePartDto.Part;

            var cityList = currentCulture switch
            {
                Cultures.En => await _cityFromStorageGetter.GetListByNamePart(namePart, namePartDto.CountryId),
                Cultures.Ru => await _cityFromStorageGetter.GetListByNamePartRu(namePart, namePartDto.CountryId),
                _ => await _cityFromStorageGetter.GetListByNamePartRu(namePart, namePartDto.CountryId)
            };

            var cityDtoList = cityList.Select(x => new CityDto
            {
                Id = x.Id,
                Name = $"{x.Name}, {x.Country.Name}",
                Country = x.Country
            });
        
            return cityDtoList;
        }   
    }
}