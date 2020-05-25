using System.Collections.Generic;
using System.Threading.Tasks;
using Amver.Domain.Dto;

namespace Amver.WebApi.Interfaces.Services
{
    public interface ICountryService
    {
        Task<IEnumerable<CountryDto>> GetCountryListByNamePartAsync(NamePartDto namePartDto, string currentCulture);
    }
}