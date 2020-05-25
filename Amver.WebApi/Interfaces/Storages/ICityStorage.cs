using System.Collections.Generic;
using System.Threading.Tasks;
using Amver.Domain.Entities;

namespace Amver.WebApi.Interfaces.Storages
{
    public interface ICityStorage
    {
        Task<List<City>> GetCityListByNamePartAsNoTrackingAsync(string namePart, int countryId);

        Task<List<City>> GetCityListByNamePartRuAsNoTrackingAsync(string namePart, int countryId);
    }
}