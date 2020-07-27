using System.Collections.Generic;
using System.Threading.Tasks;

namespace Amver.Api.Interfaces.City
{
    public interface ICityFromStorageGetter
    {
        Task<IEnumerable<Domain.Entities.City>> GetListByNamePart(string namePart, int countryId);

        Task<IEnumerable<Domain.Entities.City>> GetListByNamePartRu(string namePart, int countryId);
    }
}    