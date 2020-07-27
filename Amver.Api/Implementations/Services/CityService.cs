using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amver.Api.Interfaces.Services;
using Amver.Api.Interfaces.Storages;
using Amver.Domain.Constants;
using Amver.Domain.Dto;
using Amver.Domain.Entities;

namespace Amver.Api.Implementations.Services
{
    public class CityService : ICityService
    {
        // private readonly ICityStorage _cityStorage;
        // public CityService(ICityStorage cityStorage)
        // {
        //     _cityStorage = cityStorage ?? throw new ArgumentNullException(nameof(cityStorage));
        // }
        //
        // public async Task<IEnumerable<CityDto>> GetCityListByNamePartAsync(NamePartDto namePartDto, string currentCulture)
        // {
        //     if (namePartDto == null) throw new ArgumentNullException(nameof(namePartDto));
        //     if (string.IsNullOrEmpty(namePartDto.Part)) throw new ArgumentNullException(nameof(namePartDto.Part));
        //     
        //     var namePart = namePartDto.Part;
        //     
        //     List<Domain.Entities.City> cityList;
        //
        //     switch (currentCulture)
        //     {
        //         case Cultures.En: 
        //             cityList = await _cityStorage.GetCityListByNamePart(namePart, namePartDto.CountryId);
        //             break;
        //         case Cultures.Ru: 
        //             cityList = await _cityStorage.GetCityListByNamePartRu(namePart, namePartDto.CountryId);
        //             break;
        //         default:
        //             cityList = await _cityStorage.GetCityListByNamePartRu(namePart, namePartDto.CountryId);
        //             break;
        //     }
        //     
        //     var cityDtoList = cityList.Select(x => new CityDto
        //     {
        //         Id = x.Id,
        //         Name = $"{x.Name}, {x.Country.Name}",
        //         Country = x.Country
        //     });
        //
        //     return cityDtoList;
        // }
    }
}