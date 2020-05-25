using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.Domain.Dto;
using Amver.Domain.Models;
using Amver.Libraries.Network.Interfaces;
using Amver.MobileClient.Localization;
using Amver.MobileClient.Models;
using Autofac;
using Newtonsoft.Json;
using Xamarin.Forms;
using Genders = Amver.Domain.StaticMembers.Genders;

namespace Amver.MobileClient.ViewModels
{
    public class TripViewModel : INotifyPropertyChanged
    {
        private readonly INetwork _network;
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        private TripForView _trip;
        
        public TripForView Trip
        {
            get => _trip;
            private set
            {
                _trip = value;
                OnPropertyChanged(nameof(Trip));
            }
        }

        public int UserId { get; set; }
        
        private bool _isFavouriteTrip;
        public bool IsFavouriteTrip
        {
            get => _isFavouriteTrip;
            set
            {
                _isFavouriteTrip = value;
                OnPropertyChanged(nameof(IsFavouriteTrip));
            }
        }
        
        public bool IsModalAuthWasOpen { get; set; }
        
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }
        
        private string _heartStatus;
        [DefaultValue(Heart)]
        public string HeartStatus
        {
            get => _heartStatus;
            set
            {
                _heartStatus = value;
                OnPropertyChanged(nameof(HeartStatus));
            }
        }
        
        private string _favoriteStatus;
        public string FavoriteStatus
        {
            get => _favoriteStatus;
            set
            {
                _favoriteStatus = value;
                OnPropertyChanged(nameof(FavoriteStatus));
            }
        }

        private const string FavouriteHeart = "heartcard36dark.png";
        private const string Heart = "heart36dark.png";
        private readonly string _inFavourites = AppResources.InFavorites;
        private readonly string _toFavourites = AppResources.ToFavorites;
        public TripViewModel(int tripId)  
        {
            _network = App.Container.Resolve<INetwork>();
        }
        
        public void OnPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public async Task<(BaseResult baseResult, string content)> LoadTripAsync(int tripId)
        {
            if (tripId <= 0) throw new ArgumentOutOfRangeException(nameof(tripId));
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess || string.IsNullOrEmpty(bearerTokenObj.ToString()))
            { 
                var fullUrl = $"{Url.Trip}{tripId}";
                var result = await _network.LoadDataGetAsync(fullUrl, null).ConfigureAwait(false);
                return result;
            }
            else
            {
                if (!(bearerTokenObj is string bearerToken))
                    return (new BaseResult { Result = StatusCode.Unauthorized, Message = Messages.BadToken }, null);
            
                var fullUrl = $"{Url.GetAuthTrip}{tripId}";
                var result = await _network.LoadDataGetAsync(fullUrl, bearerToken).ConfigureAwait(false);
                return result;
            }
        } 
        
        public void FillTrip(string content)
        {
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException("Value cannot be null or empty.", nameof(content));
            
            var trip = JsonConvert.DeserializeObject<TripDto>(content);
            //ToDo implement start modal to insert birthday email

            var tripForView = new TripForView
            {
                UserName = $"{trip.UserFirsName} >",
                UserAge = trip.UserBirthDay == null ? string.Empty : $"{AppResources.UserAge} {CalculateAge(trip.UserBirthDay.Value)}",
                FromCountry = trip.FromCountry,
                FromCity = trip.FromCity,
                ToCountry = trip.ToCountry,
                ToCity = string.IsNullOrEmpty(trip.ToCity) ? "-" : trip.ToCity,
                PreferredGender = Genders.GenderList.First(x=>x.Key == trip.PreferredGender).Value,
                DateFrom = trip.DateFrom.ToString("D"),
                DateTo = trip.DateTo.ToString("D"),
                CreatedDate = trip.CreatedDate.ToString("D"),
                Comment = trip.Comment,
                UserLogin = trip.UserLogin,
                UserId = trip.UserId,
                ImageUri = new UriImageSource
                {
                    CachingEnabled = true,
                    CacheValidity = new TimeSpan(30, 0, 0, 0),
                    Uri = new Uri(trip.ImageUri)
                },
                UserImageUri = new UriImageSource
                {
                    CachingEnabled = true,
                    CacheValidity = new TimeSpan(30, 0, 0, 0),
                    Uri = new Uri(trip.UserImageUri)
                }
            };
            Trip = tripForView;
            UserId = trip.UserId;
        }
        
        private static int CalculateAge(DateTime dateOfBirth)  
        {
            var age = DateTime.UtcNow.Year - dateOfBirth.Year;  
            if (DateTime.UtcNow.DayOfYear < dateOfBirth.DayOfYear)  
                age -= 1;  
  
            return age;  
        }

        
        public async Task<(BaseResult baseResult, string content)> LoadIsFavouriteTripAsync(int tripId)
        {
            if (tripId <= 0) throw new ArgumentOutOfRangeException(nameof(tripId));

            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult {Result = StatusCode.Unauthorized, Message = Messages.BadToken}, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult {Result = StatusCode.Error, Message = Messages.BadToken}, null);
            
            var favouriteTripDto = new FavouriteTripDto
            {
                TripId = tripId
            };
            
            var serializedFavouriteTrip = JsonConvert.SerializeObject(favouriteTripDto);
            var result = await _network.LoadDataPostAsync(Url.IsFavouriteTrip, serializedFavouriteTrip, bearerToken).ConfigureAwait(false);

            return result;
        }

        public void FillHeart(string content)
        {
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException("Value cannot be null or empty.", nameof(content));
            
            var favouriteTrip = JsonConvert.DeserializeObject<FavouriteTripDto>(content);

            IsFavouriteTrip = favouriteTrip.IsFavourite;
            HeartStatus = IsFavouriteTrip ? FavouriteHeart : Heart;
            FavoriteStatus = IsFavouriteTrip ? _inFavourites : _toFavourites;
        }

        public async Task<(BaseResult baseResult, string content)> HeartTapAsync(int tripId)
        {
            if (tripId <= 0) throw new ArgumentOutOfRangeException(nameof(tripId));
            
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult {Result = StatusCode.Unauthorized, Message = Messages.BadToken}, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult {Result = StatusCode.Error, Message = Messages.BadToken}, null);
            
            var favouriteTripDto = new FavouriteTripDto
            {
                TripId = tripId,
                IsFavourite = !IsFavouriteTrip
            };
            IsFavouriteTrip = !IsFavouriteTrip;
            HeartStatus = IsFavouriteTrip ? FavouriteHeart : Heart;
            FavoriteStatus = IsFavouriteTrip ? _inFavourites : _toFavourites;
            var serializedFavouriteTrip = JsonConvert.SerializeObject(favouriteTripDto);

            var result = await _network.LoadDataPostAsync(Url.Like, serializedFavouriteTrip, bearerToken);

            return result;
        }
        
        public void FillHeartIfTapped(string content)
        {
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException("Value cannot be null or empty.", nameof(content));
            IsFavouriteTrip = !IsFavouriteTrip;

            var favouriteTrip = JsonConvert.DeserializeObject<FavouriteTripDto>(content);

            IsFavouriteTrip = favouriteTrip.IsFavourite;
        }
    }
}