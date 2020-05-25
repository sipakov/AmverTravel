using System.Collections.Generic;
using System.Threading.Tasks;
using Amver.Domain.Dto;
using Amver.Domain.Entities;
using Amver.Domain.Models;

namespace Amver.WebApi.Interfaces.Storages
{
    public interface ITripStorage
    {
        Task<BaseResult> CreateAsync(Trip trip);
        
        Task<IEnumerable<Trip>> GetByUserIdAsync(int userId);

        Task<IEnumerable<Trip>> GetListAsync(FilterTripRequest filterTripRequest);
        
        Task<Trip> GetByIdAsync(int tripId);
        
        Task<BaseResult> UpdateAsync(Trip trip);
        
        Task<BaseResult> RemoveAsync(int tripId);

        Task<BaseResult> CompleteAsync(int tripId);
        
        Task<IEnumerable<Trip>> GetAuthorizedListAsync(FilterTripRequest filterTripRequest, int userId);
    }
}