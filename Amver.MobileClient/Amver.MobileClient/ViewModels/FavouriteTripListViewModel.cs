using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.Domain.Models;
using Amver.Libraries.Network.Interfaces;
using Amver.MobileClient.Models;
using Autofac;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace Amver.MobileClient.ViewModels
{
    public class FavouriteTripListViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly INetwork _network;

        private IEnumerable<TripForList> _favouriteTrips;

        public IEnumerable<TripForList> FavouriteTrips
        {
            get => _favouriteTrips; 
            set
            {
                _favouriteTrips = value;
                OnPropertyChanged(nameof(FavouriteTrips));
            }
        }

        public bool IsModalAuthWasOpen { get; set; }

        private bool _isNotEmptyList;

        public bool IsNotEmptyList
        {
            get => _isNotEmptyList;
            set
            {
                _isNotEmptyList = value;
                OnPropertyChanged(nameof(IsNotEmptyList));
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

        public FavouriteTripListViewModel()
        {
            _network = App.Container.Resolve<INetwork>();
        }

        public void OnPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        //ToDo implement date for current culture correct 
        public async Task<(BaseResult baseResult, string content)> LoadFavouriteTripListAsync()
        {
            IsBusy = true;
            var fullUrl = $"{Url.FavouriteTripList}";
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult {Result = StatusCode.Unauthorized}, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult {Result = StatusCode.Unauthorized}, null);

            var result = await _network.LoadDataGetAsync(fullUrl, bearerToken);
            return result;    
        }

        public void FillFavouriteTripLists(string content)
        {
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException("Value cannot be null or empty.", nameof(content));

            var favouriteTripList = JsonConvert.DeserializeObject<FavouriteTripResponse>(content);

            FavouriteTrips = favouriteTripList.Trips.Select(x => new TripForList
            {
                Id = x.TripId,
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
            if (_favouriteTrips != null && _favouriteTrips.Any()) IsNotEmptyList = true;
            IsBusy = false;
        }
    }
}