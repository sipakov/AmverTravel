using System.Collections.Generic;
using System.Threading.Tasks;
using Amver.Domain.Entities;

namespace Amver.WebApi.Interfaces.Storages
{
    public interface ICountryStorage
    {
        Task<List<Country>> GetCountryListByNamePartAsNoTrackingAsync(string namePart);

        Task<List<Country>> GetCountryListByNamePartRuAsNoTrackingAsync(string namePart);
    }
}