using System.Collections.Generic;
using System.Threading.Tasks;
using Amver.Domain.Dto;

namespace Amver.WebApi.Interfaces.Services
{
    public interface ICityService
    {
        Task<IEnumerable<CityDto>> GetCityListByNamePartAsync(NamePartDto namePartDto, string currentCulture);
    }
}