using System.Threading.Tasks;
using Amver.Domain.Dto;
using Amver.Domain.Entities;
using Amver.Domain.Models;

namespace Amver.WebApi.Interfaces.Services
{
    public interface ITripService
    {
        Task<BaseResult> CreateAsync(TripDto tripDto);

        Task<TripResponse> GetListByUserIdAsync(int userId, string currentCulture);

        Task<TripResponse> GetListAsync(FilterTripRequest filterTripRequest, string currentCulture);
        
        Task<TripDto> GetByIdAsync(int tripId, string currentCulture);

        Task<BaseResult> UpdateAsync(TripDto tripDto);
        
        Task<BaseResult> RemoveAsync(int tripId);
        
        Task<BaseResult> CompleteAsync(int tripId);

        Task<TripResponse> GetAuthorizedListAsync(FilterTripRequest filterTripRequest, int userId, string currentCulture);
    }
}