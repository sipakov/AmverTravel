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
    public class MyTripViewModel : INotifyPropertyChanged
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

        public bool IsModalAuthWasOpen { get; set; }

        public MyTripViewModel()
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

            var isSuccess =
                Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult {Result = StatusCode.Unauthorized, Message = Messages.BadToken}, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult {Result = StatusCode.Unauthorized, Message = Messages.BadToken}, null);

            var fullUrl = $"{Url.MyTrip}{tripId}";

            var result = await _network.LoadDataGetAsync(fullUrl, bearerToken);

            return result;
        }

        public void FillTrip(string content)
        {
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException("Value cannot be null or empty.", nameof(content));

            var trip = JsonConvert.DeserializeObject<TripDto>(content);

            var tripForView = new TripForView
            {
                UserName = $"{trip.UserFirsName} {trip.UserLastName}",
                UserAge = trip.UserBirthDay == null
                    ? string.Empty
                    : $"{AppResources.UserAge} {CalculateAge(trip.UserBirthDay.Value)}",
                FromCountry = trip.FromCountry,
                FromCity = trip.FromCity,
                ToCountry = trip.ToCountry,
                ToCity = string.IsNullOrEmpty(trip.ToCity) ? "-" : trip.ToCity,
                PreferredGender = Genders.GenderList.First(x => x.Key == trip.PreferredGender).Value,
                DateFrom = trip.DateFrom.ToString("D"),
                DateTo = trip.DateTo.ToString("D"),
                CreatedDate = trip.CreatedDate.ToString("D"),
                Comment = trip.Comment,
                IsCompleted = trip.IsCompleted,
                IsCanModify = !trip.IsCompleted,
                ImageUri = new UriImageSource
                {
                    CachingEnabled = true,
                    CacheValidity = new TimeSpan(30, 0, 0, 0),
                    Uri = new Uri(trip.ImageUri)
                }
            };
            Trip = tripForView;
        }

        private static int CalculateAge(DateTime dateOfBirth)
        {
            var age = DateTime.UtcNow.Year - dateOfBirth.Year;
            if (DateTime.UtcNow.DayOfYear < dateOfBirth.DayOfYear)
                age -= 1;

            return age;
        }

        public async Task<(BaseResult baseResult, string content)> RemoveTripAsync(int tripId)
        {
            if (tripId <= 0) throw new ArgumentOutOfRangeException(nameof(tripId));

            var isSuccess =
                Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult {Result = StatusCode.Unauthorized, Message = Messages.BadToken}, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult {Result = StatusCode.Unauthorized, Message = Messages.BadToken}, null);

            var fullUrl = $"{Url.RemoveTrip}{tripId}";

            var result = await _network.LoadDataGetAsync(fullUrl, bearerToken);

            return result;
        }


        public async Task<(BaseResult baseResult, string content)> CompleteFileAsync(int tripId)
        {
            if (tripId <= 0) throw new ArgumentOutOfRangeException(nameof(tripId));

            var isSuccess =
                Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult {Result = StatusCode.Unauthorized, Message = Messages.BadToken}, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult {Result = StatusCode.Unauthorized, Message = Messages.BadToken}, null);

            var fullUrl = $"{Url.CompleteTrip}{tripId}";

            var result = await _network.LoadDataGetAsync(fullUrl, bearerToken);

            return result;
        }
    }
}