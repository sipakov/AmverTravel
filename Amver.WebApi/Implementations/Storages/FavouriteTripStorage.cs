using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amver.Domain.Dto;
using Amver.Domain.Entities;
using Amver.EfCli;
using Amver.WebApi.Interfaces.Storages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Amver.WebApi.Implementations.Storages
{
    public class FavouriteTripStorage : IFavouriteTripStorage
    {
        private readonly ILogger<FavouriteTripStorage> _logger;
        private readonly IContextFactory<ApplicationContext> _contextFactory;

        public FavouriteTripStorage(IContextFactory<ApplicationContext> contextFactory, ILogger<FavouriteTripStorage> logger)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public async Task<bool> LikeAsync(FavouriteTrip favouriteTrip)
        {
            if (favouriteTrip == null) throw new ArgumentNullException(nameof(favouriteTrip));
            
            using (var context = _contextFactory.CreateContext())
            {
                _logger.LogInformation($"{favouriteTrip.UserId} liked tripId {favouriteTrip.TripId}");
                var favouriteTripFromStorage = await context.FavouriteTrips.FindAsync(favouriteTrip.UserId, favouriteTrip.TripId);
                try
                {
                    if (favouriteTripFromStorage == null)
                    {
                        await context.FavouriteTrips.AddAsync(favouriteTrip);
                        await context.SaveChangesAsync();
                        return favouriteTrip.IsFavourite;
                    }
                    if (!favouriteTrip.IsFavourite)
                    {
                        favouriteTripFromStorage.IsFavourite = false;
                    }
                    else
                    {
                        favouriteTripFromStorage.IsFavourite = true;   
                    }

                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{favouriteTrip.UserId} failed to like tripId {favouriteTrip.TripId}", ex);
                    throw;
                }

                return favouriteTrip.IsFavourite;
            }
        }

        public async Task<IEnumerable<FavouriteTrip>> GetFavouriteTripList(int userId)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            using (var context = _contextFactory.CreateContext())
            {
                var queryableFavouriteTrip = context.FavouriteTrips.AsQueryable();

                queryableFavouriteTrip = queryableFavouriteTrip.Include(x => x.Trip).ThenInclude(x=>x.User);
                queryableFavouriteTrip = queryableFavouriteTrip.Include(x => x.User);
                queryableFavouriteTrip = queryableFavouriteTrip.Include(x => x.Trip.FromCity);
                queryableFavouriteTrip = queryableFavouriteTrip.Include(x => x.Trip.FromCountry);
                queryableFavouriteTrip = queryableFavouriteTrip.Include(x => x.Trip.ToCity);
                queryableFavouriteTrip = queryableFavouriteTrip.Include(x => x.Trip.ToCountry);
                
                var notAccessibleUserIds = context.UserToBlockedUsers.Where(x => x.BlockedUserId == userId).Select(x=>x.UserId);

                var favouriteTrips = await queryableFavouriteTrip.AsNoTracking().Where(x =>
                    x.UserId == userId && x.User.DeletedDate == null && x.IsFavourite && !x.Trip.IsDeleted &&
                    !x.Trip.IsBanned && !notAccessibleUserIds.Contains(x.Trip.UserId)).ToListAsync();
                return favouriteTrips;
            }
        }

        public async Task<FavouriteTrip> IsLikedTripAsync(FavouriteTrip favouriteTrip)
        {
            if (favouriteTrip == null) throw new ArgumentNullException(nameof(favouriteTrip));
            if (favouriteTrip.UserId <=0) throw new ArgumentOutOfRangeException(nameof(favouriteTrip.UserId));
            if (favouriteTrip.TripId <=0) throw new ArgumentOutOfRangeException(nameof(favouriteTrip.TripId));
            
            using (var context = _contextFactory.CreateContext())
            {
                return await context.FavouriteTrips.FindAsync(favouriteTrip.UserId, favouriteTrip.TripId);
            }
        }
    }
}