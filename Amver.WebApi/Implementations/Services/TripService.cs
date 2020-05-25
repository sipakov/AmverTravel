using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.Domain.Entities;
using Amver.Domain.Dto;
using Amver.Domain.Models;
using Amver.WebApi.Interfaces.Services;
using Amver.WebApi.Interfaces.Storages;
using Microsoft.AspNetCore.Hosting;

namespace Amver.WebApi.Implementations.Services
{
    public class TripService : ITripService
    {
        private readonly ITripStorage _tripStorage;
        private readonly IHostingEnvironment _appEnvironment;

        public TripService(ITripStorage tripStorage, IHostingEnvironment appEnvironment)
        {
            _tripStorage = tripStorage ?? throw new ArgumentNullException(nameof(tripStorage));
            _appEnvironment = appEnvironment;
        }

        public async Task<BaseResult> CreateAsync(TripDto tripDto)
        {
            if (tripDto == null) throw new ArgumentNullException(nameof(tripDto));
            if (tripDto.FromCityId == null) throw new ArgumentNullException(nameof(tripDto.FromCityId));
            if (tripDto.ToCountryId == null) throw new ArgumentNullException(nameof(tripDto.ToCountryId));

            var trip = new Trip
            {
                FromCityId = tripDto.FromCityId.Value,
                FromCountryId = tripDto.FromCountryId,
                ToCityId = tripDto.ToCityId <= 0 ? null : tripDto.ToCityId,
                ToCountryId = tripDto.ToCountryId.Value,
                UserId = tripDto.UserId,
                DateFrom = tripDto.DateFrom,
                DateTo = tripDto.DateTo,
                PreferredGender = tripDto.PreferredGender,
                Comment = tripDto.Comment,
                CreatedDate = DateTime.UtcNow
            };

            return await _tripStorage.CreateAsync(trip);
        }

        public async Task<TripResponse> GetListByUserIdAsync(int userId, string currentCulture)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            var trips = await _tripStorage.GetByUserIdAsync(userId);

            var pathToDirectory = Path.Combine(_appEnvironment.ContentRootPath, "wwwroot", "Photos", "Countries");
            var pathToUserDirectory = Path.Combine(_appEnvironment.WebRootPath, "Photos", "Users");

            var tripsForListDto = new List<TripForListDto>();

            foreach (var trip in trips)
            {
                var tripDto = new TripForListDto
                {
                    Id = trip.Id,
                    UserName = trip.User.FirstName,
                    DateFrom = trip.DateFrom,
                    DateTo = trip.DateTo,
                    IsCompleted = trip.IsCompleted,
                    IsDeleted = trip.IsDeleted,
                    UserLogin = trip.User.Login,
                    ImageUri = $"https://www.amver.net/Photos/Countries/{trip.ToCountry.Name}/{trip.ToCountry.Name}_main.png"
                };
                switch (currentCulture)
                {
                    case Cultures.Ru:
                        tripDto.FromCity = trip.FromCity.ruRu;
                        tripDto.FromCountry = trip.FromCountry == null ? string.Empty : trip.FromCity.Country.ruRu;
                        tripDto.ToCity = trip.ToCity == null ? string.Empty : trip.ToCity.ruRu;
                        tripDto.ToCountry = trip.ToCountry.ruRu;
                        break;
                    default:
                        tripDto.FromCity = trip.FromCity.Name;
                        tripDto.FromCountry = trip.FromCountry == null ? string.Empty : trip.FromCity.Country.Name;
                        tripDto.ToCity = trip.ToCity == null ? string.Empty : trip.ToCity.Name;
                        tripDto.ToCountry = trip.ToCountry.Name;
                        break;
                }
                tripsForListDto.Add(tripDto);
            }

            var tripResponse = new TripResponse
            {
                Trips = tripsForListDto,
                Count = tripsForListDto.Count
            };
            return tripResponse;
        }

        public async Task<TripResponse> GetListAsync(FilterTripRequest filterTripRequest, string currentCulture)
        {
            if (filterTripRequest == null) throw new ArgumentNullException(nameof(filterTripRequest));

            var trips = await _tripStorage.GetListAsync(filterTripRequest);
            
            var pathToUserDirectory = Path.Combine(_appEnvironment.WebRootPath, "Photos", "Users");

            var tripsForListDto = new List<TripForListDto>();

            foreach (var trip in trips)
            {
                var targetPath = Path.Combine(pathToUserDirectory, $"{trip.User.Login}");
                var targetImages = new List<string>();
                if (Directory.Exists(targetPath))
                {
                    targetImages = Directory.GetFiles(targetPath, "*_icon.jpeg").ToList();
                }
                var tripDto = new TripForListDto
                {
                    Id = trip.Id,
                    UserName = trip.User.FirstName,
                    DateFrom = trip.DateFrom,
                    DateTo = trip.DateTo,
                    IsCompleted = trip.IsCompleted,
                    IsDeleted = trip.IsDeleted,
                    UserLogin = trip.User.Login,
                    ImageUri = $"https://www.amver.net/Photos/Countries/{trip.ToCountry.Name}/{trip.ToCountry.Name}_main.png",
                    UserImageUri = targetImages.Any() ? $"https://www.amver.net/Photos/Users/{trip.User.Login}/{Path.GetFileName(targetImages.First())}" : Path.Combine("https://www.amver.net", "images", "userAccountIcon.png")
                };
                switch (currentCulture)
                {
                    case Cultures.Ru:
                        tripDto.FromCity = trip.FromCity.ruRu;
                        tripDto.FromCountry = trip.FromCountry == null ? string.Empty : trip.FromCity.Country.ruRu;
                        tripDto.ToCity = trip.ToCity == null ? string.Empty : trip.ToCity.ruRu;
                        tripDto.ToCountry = trip.ToCountry.ruRu;
                        break;
                    default:
                        tripDto.FromCity = trip.FromCity.Name;
                        tripDto.FromCountry = trip.FromCountry == null ? string.Empty : trip.FromCity.Country.Name;
                        tripDto.ToCity = trip.ToCity == null ? string.Empty : trip.ToCity.Name;
                        tripDto.ToCountry = trip.ToCountry.Name;
                        break;
                }
                tripsForListDto.Add(tripDto);
            }

            var tripResponse = new TripResponse
            {
                Trips = tripsForListDto,
                Count = tripsForListDto.Count
            };

            return tripResponse;
        }
        
         public async Task<TripResponse> GetAuthorizedListAsync(FilterTripRequest filterTripRequest, int userId, string currentCulture)
        {
            if (filterTripRequest == null) throw new ArgumentNullException(nameof(filterTripRequest));

            var trips = await _tripStorage.GetAuthorizedListAsync(filterTripRequest, userId);

            var pathToUserDirectory = Path.Combine(_appEnvironment.WebRootPath, "Photos", "Users");

            var tripsForListDto = new List<TripForListDto>();

            foreach (var trip in trips)
            {
                var targetPath = Path.Combine(pathToUserDirectory, $"{trip.User.Login}");
                var targetImages = new List<string>();
                if (Directory.Exists(targetPath))
                {
                    targetImages = Directory.GetFiles(targetPath, "*_icon.jpeg").ToList();
                }
                var tripDto = new TripForListDto
                {
                    Id = trip.Id,
                    UserName = trip.User.FirstName,
                    DateFrom = trip.DateFrom,
                    DateTo = trip.DateTo,
                    IsCompleted = trip.IsCompleted,
                    IsDeleted = trip.IsDeleted,
                    UserLogin = trip.User.Login,
                    ImageUri = $"https://www.amver.net/Photos/Countries/{trip.ToCountry.Name}/{trip.ToCountry.Name}_main.png",
                    UserImageUri = targetImages.Any() ? $"https://www.amver.net/Photos/Users/{trip.User.Login}/{Path.GetFileName(targetImages.First())}" : Path.Combine("https://www.amver.net", "images", "userAccountIcon.png")
                };
                switch (currentCulture)
                {
                    case Cultures.Ru:
                        tripDto.FromCity = trip.FromCity.ruRu;
                        tripDto.FromCountry = trip.FromCountry == null ? string.Empty : trip.FromCity.Country.ruRu;
                        tripDto.ToCity = trip.ToCity == null ? string.Empty : trip.ToCity.ruRu;
                        tripDto.ToCountry = trip.ToCountry.ruRu;
                        break;
                    default:
                        tripDto.FromCity = trip.FromCity.Name;
                        tripDto.FromCountry = trip.FromCountry == null ? string.Empty : trip.FromCity.Country.Name;
                        tripDto.ToCity = trip.ToCity == null ? string.Empty : trip.ToCity.Name;
                        tripDto.ToCountry = trip.ToCountry.Name;
                        break;
                }
                tripsForListDto.Add(tripDto);
            }

            var tripResponse = new TripResponse
            {
                Trips = tripsForListDto,
                Count = tripsForListDto.Count
            };

            return tripResponse;
        }

        public async Task<TripDto> GetByIdAsync(int tripId, string currentCulture)
        {
            if (tripId <= 0) throw new ArgumentOutOfRangeException(nameof(tripId));

            var trip = await _tripStorage.GetByIdAsync(tripId);

            var pathToUserDirectory =
                Path.Combine(_appEnvironment.WebRootPath, "Photos", "Users", $"{trip.User.Login}");
            var pathToCountryImageDirectory =
                Path.Combine(_appEnvironment.ContentRootPath, "wwwroot", "Photos", "Countries");
            
            var targetImages = new List<string>();
            if (Directory.Exists(pathToUserDirectory))
            {
                targetImages = Directory.GetFiles(pathToUserDirectory, "*_icon.jpeg").ToList();
            }
            var tripDto = new TripDto
            {
                Id = trip.Id,
                FromCity = trip.FromCity.Name,
                FromCityId = trip.FromCity?.Id,
                FromCountry = trip.FromCity.Country == null ? string.Empty : trip.FromCity.Country.Name,
                FromCountryId = trip.FromCountry?.Id,
                ToCity = trip.ToCity == null ? string.Empty : trip.ToCity.Name,
                ToCityId = trip.ToCity?.Id,
                ToCountry = trip.ToCountry.Name,
                ToCountryId = trip.ToCountry?.Id,
                UserFirsName = trip.User.FirstName,
                UserLastName = trip.User.LastName,
                UserId = trip.UserId,
                UserLogin = trip.User.Login,
                UserBirthDay = trip.User.BirthDay,
                DateFrom = trip.DateFrom,
                DateTo = trip.DateTo,
                CreatedDate = trip.CreatedDate,
                PreferredGender = trip.PreferredGender,
                Comment = trip.Comment,
                IsCompleted = trip.IsCompleted,
                ImageUri = $"https://www.amver.net/Photos/Countries/{trip.ToCountry.Name}/{trip.ToCountry.Name}_main.png",
                UserImageUri = targetImages.Any() ? $"https://www.amver.net/Photos/Users/{trip.User.Login}/{Path.GetFileName(targetImages.First())}" : Path.Combine("https://www.amver.net", "images", "userAccountIcon.png")
            };
            switch (currentCulture)
            {
                case Cultures.Ru:
                    tripDto.FromCity = trip.FromCity.ruRu;
                    tripDto.FromCountry = trip.FromCity.Country == null ? string.Empty : trip.FromCity.Country.ruRu;
                    tripDto.ToCity = trip.ToCity == null ? string.Empty : trip.ToCity.ruRu;
                    tripDto.ToCountry = trip.ToCountry.ruRu;
                    break;
                default:
                    tripDto.FromCity = trip.FromCity.Name;
                    tripDto.FromCountry = trip.FromCity.Country == null ? string.Empty : trip.FromCity.Country.Name;
                    tripDto.ToCity = trip.ToCity == null ? string.Empty : trip.ToCity.Name;
                    tripDto.ToCountry = trip.ToCountry.Name;
                    break;
            }

            return tripDto;
        }

        public async Task<BaseResult> UpdateAsync(TripDto tripDto)
        {
            if (tripDto == null) throw new ArgumentNullException(nameof(tripDto));
            if (tripDto.FromCityId == null) throw new ArgumentNullException(nameof(tripDto.FromCityId));
            if (tripDto.ToCountryId == null) throw new ArgumentNullException(nameof(tripDto.ToCountryId));

            var trip = new Trip
            {
                Id = tripDto.Id,
                DateTo = tripDto.DateTo,
                DateFrom = tripDto.DateFrom,
                PreferredGender = tripDto.PreferredGender,
                Comment = tripDto.Comment,
                FromCityId = tripDto.FromCityId.Value,
                ToCityId = tripDto.ToCityId,
                ToCountryId = tripDto.ToCountryId.Value,
                UserId = tripDto.UserId
            };

            return await _tripStorage.UpdateAsync(trip);
        }

        public async Task<BaseResult> RemoveAsync(int tripId)
        {
            if (tripId <= 0) throw new ArgumentOutOfRangeException(nameof(tripId));

            return await _tripStorage.RemoveAsync(tripId);
        }

        public async Task<BaseResult> CompleteAsync(int tripId)
        {
            if (tripId <= 0) throw new ArgumentOutOfRangeException(nameof(tripId));

            return await _tripStorage.CompleteAsync(tripId);
        }

        private string GetCityNameByCulture(Trip trip, string currentCulture)
        {
            return null;
        }
    }
}