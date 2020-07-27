using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amver.Api.Interfaces.Services;
using Amver.Api.Interfaces.Storages;
using Amver.Domain.Constants;
using Amver.Domain.Dto;
using Amver.Domain.Entities;
using Amver.Domain.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Amver.Api.Implementations.Services
{
    public class FavouriteTripService : IFavouriteTripService
    {
        private readonly ILogger<FavouriteTripService> _logger;
        private readonly IFavouriteTripStorage _favouriteTripStorage;
        private readonly IHostingEnvironment _appEnvironment;

        public FavouriteTripService(IFavouriteTripStorage favouriteTripStorage, ILogger<FavouriteTripService> logger, IHostingEnvironment appEnvironment)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appEnvironment = appEnvironment ?? throw new ArgumentNullException(nameof(appEnvironment));
            if (favouriteTripStorage != null) _favouriteTripStorage = favouriteTripStorage;
        }

        public async Task<FavouriteTripDto> LikeAsync(FavouriteTripDto favouriteTripDto)
        {
            if (favouriteTripDto == null) throw new ArgumentNullException(nameof(favouriteTripDto));

            var favouriteTrip = new FavouriteTrip
            {
                UserId = favouriteTripDto.UserId,
                TripId = favouriteTripDto.TripId,
                IsFavourite = favouriteTripDto.IsFavourite
            };

            var result = await _favouriteTripStorage.LikeAsync(favouriteTrip);

            favouriteTripDto.IsFavourite = result;
            return favouriteTripDto;
        }

        public async Task<FavouriteTripResponse> GetFavouriteTripList(int userId, string currentCulture)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            var favouriteTrips = await _favouriteTripStorage.GetFavouriteTripList(userId);

            var pathToDirectory = Path.Combine(_appEnvironment.ContentRootPath, "wwwroot", "Photos", "Countries");
            var pathToUserDirectory = Path.Combine(_appEnvironment.WebRootPath, "Photos", "Users");

            var favouriteTripsForListDto = new List<FavouriteTripForListDto>();

            foreach (var trip in favouriteTrips)
            {
                var targetPath = Path.Combine(pathToUserDirectory, $"{trip.Trip.User.Login}");
                var targetImages = new List<string>();
                if (Directory.Exists(targetPath))
                {
                    targetImages = Directory.GetFiles(targetPath, "*_icon.jpeg").ToList();
                }
                var tripDto = new FavouriteTripForListDto
                {
                    TripId = trip.TripId,
                    UserName = trip.Trip.User.FirstName,
                    DateFrom = trip.Trip.DateFrom,
                    DateTo = trip.Trip.DateTo,
                    ImageUri = $"https://www.amver.net/Photos/Countries/{trip.Trip.ToCountry.Name}/{trip.Trip.ToCountry.Name}_main.png",
                    UserImageUri = targetImages.Any() ? $"https://www.amver.net/Photos/Users/{trip.Trip.User.Login}/{Path.GetFileName(targetImages.First())}" : Path.Combine("https://www.amver.net", "images", "userAccountIcon.png")
                };
                switch (currentCulture)
                {
                    case Cultures.Ru:
                        tripDto.FromCity = trip.Trip.FromCity.ruRu;
                        tripDto.FromCountry = trip.Trip.FromCountry == null ? string.Empty : trip.Trip.FromCity.Country.ruRu;
                        tripDto.ToCity = trip.Trip.ToCity == null ? string.Empty : trip.Trip.ToCity.ruRu;
                        tripDto.ToCountry = trip.Trip.ToCountry.ruRu;
                        break;
                    default:
                        tripDto.FromCity = trip.Trip.FromCity.Name;
                        tripDto.FromCountry = trip.Trip.FromCountry == null ? string.Empty : trip.Trip.FromCity.Country.Name;
                        tripDto.ToCity = trip.Trip.ToCity == null ? string.Empty : trip.Trip.ToCity.Name;
                        tripDto.ToCountry = trip.Trip.ToCountry.Name;
                        break;
                }
                favouriteTripsForListDto.Add(tripDto);
            }

            var favouriteTripResponse = new FavouriteTripResponse
            {
                Trips = favouriteTripsForListDto,
                Count = favouriteTripsForListDto.Count
            };
            return favouriteTripResponse;
        }

        public async Task<FavouriteTripDto> IsLikedTripAsync(FavouriteTripDto favouriteTripDto)
        {
            if (favouriteTripDto == null) throw new ArgumentNullException(nameof(favouriteTripDto));
            if (favouriteTripDto.UserId <= 0) throw new ArgumentOutOfRangeException(nameof(favouriteTripDto.UserId));
            if (favouriteTripDto.TripId <= 0) throw new ArgumentOutOfRangeException(nameof(favouriteTripDto.TripId));

            var favouriteTrip = new FavouriteTrip
            {
                UserId = favouriteTripDto.UserId,
                TripId = favouriteTripDto.TripId
            };

            var favouriteTripResponse = await _favouriteTripStorage.IsLikedTripAsync(favouriteTrip);

            if (favouriteTripResponse == null)
            {
                favouriteTripDto.IsFavourite = false;
            }
            else
            {
                favouriteTripDto.IsFavourite = favouriteTripResponse.IsFavourite;
            }

            return favouriteTripDto;
        }
    }
}