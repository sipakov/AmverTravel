using System;
using System.Threading.Tasks;

namespace FillCityAndCountryTables.Interfaces.Services
{
    public interface IAggregatorService
    {
        Task AggregateAsync(string fileName);
    }
}
