using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.Domain.Entities;
using Amver.Domain.Models;
using Amver.EfCli;
using Amver.WebApi.CustomExceptionMiddleware;
using Amver.WebApi.Interfaces.Storages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Amver.WebApi.Implementations.Storages
{
    public class TripStorage : ITripStorage
    {
        private readonly ILogger<FavouriteTripStorage> _logger;
        private readonly IContextFactory<ApplicationContext> _contextFactory;
        private readonly IStringLocalizer<AppResources> _stringLocalizer;


        public TripStorage(IContextFactory<ApplicationContext> contextFactory, ILogger<FavouriteTripStorage> logger, IStringLocalizer<AppResources> stringLocalizer)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _stringLocalizer = stringLocalizer;
        }

        public async Task<BaseResult> CreateAsync(Trip trip)
        {
            if (trip == null) throw new ArgumentNullException(nameof(trip));
            try
            {
                using (var context = _contextFactory.CreateContext())
                {
                    var activeUserTrips = await context.Trips.AsNoTracking()
                        .Where(x => x.UserId == trip.UserId && !x.IsDeleted && !x.IsCompleted).ToListAsync();
                    if (activeUserTrips.Count > 1)
                    {   
                        throw new ValidationException(_stringLocalizer["TripsCountConstraint"]);
                    }

                    await context.Trips.AddAsync(trip);
                    await context.SaveChangesAsync();

                    _logger.LogInformation($"{trip.UserId} created the trip id: {trip.Id}");
                    return new BaseResult
                    {
                        Result = StatusCode.Ok,
                        Message = Messages.Success
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{trip.UserId} failed to create trip", ex);
                throw;
            }
        }

        public async Task<IEnumerable<Trip>> GetByUserIdAsync(int userId)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            using (var context = _contextFactory.CreateContext())
            {
                var queryableTrip = context.Trips.AsQueryable();

                queryableTrip = queryableTrip.Include(x => x.ToCountry);
                queryableTrip = queryableTrip.Include(x => x.ToCity);
                queryableTrip = queryableTrip.Include(x => x.FromCity);
                queryableTrip = queryableTrip.Include(x => x.FromCountry);
                queryableTrip = queryableTrip.Include(x => x.User);

                return await queryableTrip.AsNoTracking()
                    .Where(x => x.UserId == userId && !x.IsDeleted)
                    .OrderBy(x => x.DateFrom).ToListAsync();
            }
        }

        public async Task<IEnumerable<Trip>> GetListAsync(FilterTripRequest filterTripRequest)
        {
            if (filterTripRequest == null) throw new ArgumentNullException(nameof(filterTripRequest));

            using (var context = _contextFactory.CreateContext())
            {
                var queryableTrip = context.Trips.Include(x=>x.User).AsQueryable();

                queryableTrip = GetTripQuery(queryableTrip, filterTripRequest);

                return await queryableTrip.AsNoTracking().Where(x => !x.IsCompleted && !x.IsDeleted && !x.IsBanned && !x.User.IsBanned)
                    .OrderByDescending(x => x.CreatedDate).ToListAsync();
            }
        }
        
        public async Task<IEnumerable<Trip>> GetAuthorizedListAsync(FilterTripRequest filterTripRequest, int userId)
        {
            if (filterTripRequest == null) throw new ArgumentNullException(nameof(filterTripRequest));

            using (var context = _contextFactory.CreateContext())
            {
                var queryableTrip = context.Trips.Include(x=>x.User).AsQueryable();

                queryableTrip = GetTripQuery(queryableTrip, filterTripRequest);

                var notAccessibleUserIds = context.UserToBlockedUsers.Where(x => x.BlockedUserId == userId)
                    .Select(x => x.UserId);

                return await queryableTrip.AsNoTracking().Where(x =>
                        !x.IsCompleted && !x.IsDeleted && !x.IsBanned && !x.User.IsBanned &&
                        !notAccessibleUserIds.Contains(x.UserId) && x.User.Id != userId)
                    .OrderByDescending(x => x.CreatedDate).ToListAsync();
            }
        }

        private IQueryable<Trip> GetTripQuery(IQueryable<Trip> queryableTrip, FilterTripRequest filterTripRequest)
        {
            if (queryableTrip == null) throw new ArgumentNullException(nameof(queryableTrip));
            if (filterTripRequest == null) throw new ArgumentNullException(nameof(filterTripRequest));

            queryableTrip = queryableTrip.Include(x => x.ToCountry);
            queryableTrip = queryableTrip.Include(x => x.ToCity);
            queryableTrip = queryableTrip.Include(x => x.FromCity);
            queryableTrip = queryableTrip.Include(x => x.FromCountry);


            if (filterTripRequest.ToCountry != default)
                queryableTrip = queryableTrip.Where(x => x.ToCountryId == filterTripRequest.ToCountry);

            if (filterTripRequest.ToCity != default)
                queryableTrip = queryableTrip.Where(x => x.ToCityId == filterTripRequest.ToCity);

            if (filterTripRequest.FromCountry != default)
                queryableTrip = queryableTrip.Where(x => x.FromCountryId == filterTripRequest.FromCountry);

            if (filterTripRequest.FromCity != default)
                queryableTrip = queryableTrip.Where(x => x.FromCityId == filterTripRequest.FromCity);

            if (filterTripRequest.DateFrom != default)
                queryableTrip = queryableTrip.Where(x => x.DateFrom >= filterTripRequest.DateFrom);

            if (filterTripRequest.CompanionGender != default)
                queryableTrip = queryableTrip.Where(x => x.User.GenderId == (int) filterTripRequest.CompanionGender);

            return queryableTrip;
        }

        public async Task<Trip> GetByIdAsync(int tripId)
        {
            if (tripId <= 0) throw new ArgumentOutOfRangeException(nameof(tripId));

            using (var context = _contextFactory.CreateContext())
            {
                //ToDo implement normal getting data
                return await context.Trips.AsNoTracking()
                    .Include(x => x.ToCity).Include(x => x.ToCountry)
                    .Include(x => x.User).Include(x => x.FromCity)
                    .ThenInclude(x => x.Country)
                    .FirstOrDefaultAsync(trip => trip.Id == tripId && !trip.IsDeleted);
            }
        }

        public async Task<BaseResult> UpdateAsync(Trip trip)
        {
            if (trip == null) throw new ArgumentNullException(nameof(trip));

            using (var context = _contextFactory.CreateContext())
            {
                var entityToUpdate = await context.Trips.FirstOrDefaultAsync(x => x.Id == trip.Id);
                if (entityToUpdate == null) throw new ArgumentNullException(nameof(Trip));
                try
                {
                    entityToUpdate.DateTo = trip.DateTo;
                    entityToUpdate.DateFrom = trip.DateFrom;
                    entityToUpdate.PreferredGender = trip.PreferredGender;
                    entityToUpdate.Comment = trip.Comment;
                    entityToUpdate.FromCityId = trip.FromCityId;
                    entityToUpdate.ToCityId = trip.ToCityId;
                    entityToUpdate.ToCountryId = trip.ToCountryId;

                    await context.SaveChangesAsync();
                    _logger.LogInformation($"{trip.UserId} updated the trip id: {entityToUpdate.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{trip.UserId} failed to updated the trip id: {entityToUpdate.Id}. {ex}");
                    throw;
                }
            }
            return new BaseResult();
        }

        public async Task<BaseResult> RemoveAsync(int tripId)
        {
            if (tripId <= 0) throw new ArgumentOutOfRangeException(nameof(tripId));

            using (var context = _contextFactory.CreateContext())
            {
                var entityToRemove = await context.Trips.FirstOrDefaultAsync(x => x.Id == tripId);
                if (entityToRemove == null)
                    throw new ArgumentNullException(nameof(Trip));
                try
                {
                    entityToRemove.IsDeleted = true;

                    await context.SaveChangesAsync();
                    _logger.LogInformation($"{tripId} was deleted");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to remove the trip {tripId}. {ex}");
                    throw;
                }
            }
            return new BaseResult();
        }

        public async Task<BaseResult> CompleteAsync(int tripId)
        {
            if (tripId <= 0) throw new ArgumentOutOfRangeException(nameof(tripId));
            using (var context = _contextFactory.CreateContext())
            {
                var entity = await context.Trips.FirstOrDefaultAsync(x => x.Id == tripId);

                entity.IsCompleted = true;
                await context.SaveChangesAsync();
            }
            return new BaseResult();
        }
    }
}