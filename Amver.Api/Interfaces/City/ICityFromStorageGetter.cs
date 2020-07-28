using System.Collections.Generic;
using System.Threading.Tasks;
using Amver.EfCli;

namespace Amver.Api.Interfaces.City
{
    public interface ICityFromStorageGetter
    {
        Task<IEnumerable<Domain.Entities.City>> GetListByNamePart(string namePart, int countryId, ApplicationContext context = null);

        Task<IEnumerable<Domain.Entities.City>> GetListByNamePartRu(string namePart, int countryId, ApplicationContext context = null);
    }
}    