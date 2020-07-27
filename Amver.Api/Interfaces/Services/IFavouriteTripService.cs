using System.Threading.Tasks;
using Amver.Domain.Dto;
using Amver.Domain.Models;

namespace Amver.Api.Interfaces.Services
{
    public interface IFavouriteTripService
    {
        Task<FavouriteTripDto> LikeAsync(FavouriteTripDto favouriteTripDto);

        Task<FavouriteTripResponse> GetFavouriteTripList(int userId, string currentCulture);
        
        Task<FavouriteTripDto> IsLikedTripAsync(FavouriteTripDto favouriteTripDto);
    }
}