using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Amver.Domain.Constants;
using Amver.Domain.Dto;
using Amver.Domain.Models;
using Amver.Libraries.Network.Interfaces;
using Amver.MobileClient.Localization;
using Amver.MobileClient.Models;
using Autofac;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace Amver.MobileClient.ViewModels
{
    public class TripListViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly INetwork _network;

        public ICommand SearchTripsCommand { get; }

        private IEnumerable<TripForList> _trips;

        public IEnumerable<TripForList> Trips
        {
            get => _trips;
            private set
            {
                _trips = value;
                OnPropertyChanged(nameof(Trips));
            }
        }
        public CityDto FromCity { get; set; } = new CityDto();

        public CityDto ToCity { get; set; } = new CityDto();

        public CountryDto ToCountry { get; set; } = new CountryDto();

        public DateTime DateFrom { get; set; } = DateTime.UtcNow;

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

        private bool _isVisibleSearch;
        public bool IsVisibleSearch
        {
            get => _isVisibleSearch;
            set
            {
                _isVisibleSearch = value;
                OnPropertyChanged(nameof(IsVisibleSearch));
            }
        }
        
        private bool _isRefreshing = false;
        public bool IsRefreshing { 
            get { return _isRefreshing; }
            set {
                _isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }
        
        public ICommand RefreshCommand
        {
            get {
                return new Command(async () =>
                {
                    BaseResult baseResult;
                    string content;
                    var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
                    if (!isSuccess)
                        (baseResult, content) = await LoadTripListAsync(null);
                    if (!(bearerTokenObj is string bearerToken))
                        return;
                    if (string.IsNullOrEmpty(bearerToken))
                        (baseResult, content) = await LoadTripListAsync(null);
                    else
                    {
                        (baseResult, content) = await LoadTripListAsync(bearerToken);
                    }
                    IsRefreshing = true;
                    
                    switch (baseResult.Result)
                    {
                        case StatusCode.Ok:
                            FillTripLists(content);
                            break;
                        case StatusCode.Error:
                            break;
                    }
                    IsRefreshing = false;
                });
            }
        }

        public TripListViewModel()
        {
            _network = App.Container.Resolve<INetwork>();
        }

        public void OnPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        //ToDo implement date for current culture correct
        public async Task<(BaseResult baseResult, string content)> LoadTripListAsync(string bearer)
        {
            if (FromCity.Id>0 && ToCity.Id > 0 && FromCity.Id == ToCity.Id )
                return (new BaseResult
                {
                    Result = StatusCode.Error,
                    Message = AppResources.ToCityFromCityMustBeDifferent
                }, null);
            
            if (ToCountry.Id > 0 && ToCity.Id > 0 && ToCity.Country.Id > 0 && ToCity.Country.Id != ToCountry.Id)
            {
                return (new BaseResult
                {
                    Result = StatusCode.Error,
                    Message = AppResources.ToCityToCountryAndToCountryMustBeEqual
                }, null);
            }
            var filterTripRequest = new FilterTripRequest
            {
                FromCity = FromCity.Id,
                ToCountry = ToCountry.Id <= 0 ? ToCity.Country == null ? ToCountry.Id : ToCity.Country.Id : ToCountry.Id,
                ToCity = ToCity.Id,
                DateFrom = DateFrom
            };
            if (string.IsNullOrEmpty(bearer))
            {
                var fullUrl = $"{Url.TripList}";
                var serializedFilter = JsonConvert.SerializeObject(filterTripRequest);
                var result = await _network.LoadDataPostAsync(fullUrl, serializedFilter, null);
                return result;
            }
            else
            {
                var fullUrl = $"{Url.AuthorizedTripList}";
                var serializedFilter = JsonConvert.SerializeObject(filterTripRequest);
                var result = await _network.LoadDataPostAsync(fullUrl, serializedFilter, bearer);
                return result;
            }
        }

        public void FillTripLists(string content)
        {
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException("Value cannot be null or empty.", nameof(content));

            var tripList = JsonConvert.DeserializeObject<TripResponse>(content);
         
            Trips = tripList.Trips.Select(x => new TripForList
            {
                Id = x.Id,
                UserName = x.UserName,
                FromCity = $"{x.FromCity} -> ",
                ToCity = string.IsNullOrEmpty(x.ToCity) ? $"{x.ToCountry}" : $"{x.ToCity}",
                DateFromDateTo = $"{x.DateFrom:D}",
                ImageUri = new UriImageSource
                {
                    CachingEnabled = true,
                    CacheValidity = new TimeSpan(30, 0, 0, 0),
                    Uri = new Uri(x.ImageUri)
                },
                UserImageUri = new UriImageSource
                {
                    CachingEnabled = true,
                    CacheValidity = new TimeSpan(30, 0, 0, 0),
                    Uri = new Uri(x.UserImageUri)
                }
            });
            IsBusy = false;
        }
    }
}