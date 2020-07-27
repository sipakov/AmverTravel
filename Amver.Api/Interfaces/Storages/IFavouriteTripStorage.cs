using System.Collections.Generic;
using System.Threading.Tasks;
using Amver.Domain.Entities;

namespace Amver.Api.Interfaces.Storages
{
    public interface IFavouriteTripStorage
    {
        Task<bool> LikeAsync(FavouriteTrip favouriteTrip);
        
        Task<IEnumerable<FavouriteTrip>> GetFavouriteTripList(int userId);
        
        Task<FavouriteTrip> IsLikedTripAsync(FavouriteTrip favouriteTrip);
    }
}