using System.Collections.Generic;
using System.Threading.Tasks;
using Amver.Domain.Dto;

namespace Amver.Api.Interfaces.City
{
    public interface ICityFactoryCreator
    {
        Task<IEnumerable<CityDto>> GetCityListByNamePart(NamePartDto namePartDto, string currentCulture);
    }
}