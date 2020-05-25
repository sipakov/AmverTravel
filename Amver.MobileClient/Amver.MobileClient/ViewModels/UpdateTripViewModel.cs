using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Amver.Domain.Constants;
using Amver.Domain.Dto;
using Amver.Domain.Entities;
using Amver.Domain.Enums;
using Amver.Domain.Models;
using Amver.Libraries.Network.Interfaces;
using Amver.MobileClient.Localization;
using Amver.MobileClient.Models;
using Autofac;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace Amver.MobileClient.ViewModels
{
    public class UpdateTripViewModel : INotifyPropertyChanged
    {
        private readonly INetwork _network;

        public event PropertyChangedEventHandler PropertyChanged;
        private CityDto _fromCity;
        public CityDto FromCity
        {
            get => _fromCity;
            set
            {
                _fromCity = value;
                OnPropertyChanged(nameof(FromCity));
            }
        }

        private CityDto _toCity;
        public CityDto ToCity
        {
            get => _toCity;
            set
            {
                _toCity = value;
                OnPropertyChanged(nameof(ToCity));
            }
        }

        private CountryDto _toCountry;
        public CountryDto ToCountry
        {
            get => _toCountry;
            set
            {
                _toCountry = value;
                OnPropertyChanged(nameof(ToCountry));
            }
        }

        private DateTime _dateFrom;
        public DateTime DateFrom
        {
            get => _dateFrom;
            set
            {
                _dateFrom = value;
                OnPropertyChanged(nameof(DateFrom));
            }
        }

        private DateTime _dateTo;
        public DateTime DateTo
        {
            get => _dateTo;
            set
            {
                _dateTo = value;
                OnPropertyChanged(nameof(DateTo));
            }
        }

        private string _createdDate;
        public string CreatedDate
        {
            get => _createdDate;
            set
            {
                _createdDate = value;
                OnPropertyChanged(nameof(CreatedDate));
            }
        }

        private string _preferredGender;
        public string PreferredGender
        {
            get => _preferredGender;
            set
            {
                _preferredGender = value;
                OnPropertyChanged(nameof(PreferredGender));
            }
        }
        
        private string _comment;
        public string Comment
        {
            get => _comment;
            set
            {
                _comment = value;
                OnPropertyChanged(nameof(Comment));
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

        private DateTime CurrentDate { get; set; } = DateTime.UtcNow;

        public DateTime MaximumDate { get; set; } = DateTime.UtcNow.AddYears(1);

        private List<string> _preferredGenders { get; set; }
        
        public List<string> PreferredGenders
        {
            get => _preferredGenders;
            set
            {
                _preferredGenders = value;
                OnPropertyChanged(nameof(PreferredGenders));
            }
        }

        public bool IsModalAuthWasOpen { get; set; }

        private readonly int _tripId;

        public UpdateTripViewModel(int tripId)
        {
            _tripId = tripId;
            _network = App.Container.Resolve<INetwork>();

            PreferredGenders = Domain.StaticMembers.Genders.GenderList.Values.ToList();
            FromCity = new CityDto();
            ToCity = new CityDto();
            ToCountry = new CountryDto();
        }

        private void OnPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public async Task<(BaseResult baseResult, string content)> LoadTripAsync(int tripId)
        {
            if (tripId <= 0) throw new ArgumentOutOfRangeException(nameof(tripId));

            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult {Result = StatusCode.Unauthorized, Message = Messages.BadToken}, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult {Result = StatusCode.Unauthorized, Message = Messages.BadToken}, null);

            var fullUrl = $"{Url.MyTrip}{tripId}";

            var result = await _network.LoadDataGetAsync(fullUrl, bearerToken);

            return result;
        }

        private static List<string> GetNamesPreferredGendersEnum()
        {
            var list = Domain.StaticMembers.Genders.GenderList.Values.Take(3).ToList();
            return list;
        }

        public void FillTrip(string content)
        {
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException("Value cannot be null or empty.", nameof(content));

            var trip = JsonConvert.DeserializeObject<TripDto>(content);
            //ToDo implement start modal to insert birthday email

            var fromCity = new CityDto
            {
                Id = trip.FromCityId.Value,
                Name =  $"{trip.FromCity}, {trip.FromCountry}"
            };
            FromCity = fromCity;
            var toCountry = new CountryDto
            {
                Id = trip.ToCountryId.Value,
                Name =  trip.ToCountry
            };
            ToCountry = toCountry;

            if (trip.ToCityId != null)
            {
                var toCity = new CityDto
                {
                    Id = trip.ToCityId.Value,
                    Name = $"{trip.ToCity}, {trip.ToCountry}",
                    Country = new Country
                    {
                        Id = trip.ToCountryId.Value
                    }
                };
                ToCity = toCity;
            }

            PreferredGender = Domain.StaticMembers.Genders.GenderList.First(x=>x.Key == trip.PreferredGender).Value;
            DateFrom = trip.DateFrom;
            DateTo = trip.DateTo;
            Comment = trip.Comment;
            CreatedDate = trip.CreatedDate.ToString("D");
        }

        public async Task<(BaseResult baseResult, string content)> UpdateTrip()
        {
            if (FromCity.Id <= 0)
                return (new BaseResult
                {
                    Result = StatusCode.Error,
                    Message = AppResources.FromCityIsRequired
                }, null);

            if (ToCity.Id <= 0 && ToCountry.Id <= 0)
                return (new BaseResult
                {
                    Result = StatusCode.Error,
                    Message = AppResources.ToCityOrToCountryIsRequired
                }, null);

            if (DateTo == CurrentDate || DateFrom > DateTo)
                return (new BaseResult
                {
                    Result = StatusCode.Error,
                    Message = AppResources.DateToMustBeGreatDateFrom
                }, null);
            if (FromCity.Id == ToCity.Id)
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
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult {Result = StatusCode.Unauthorized, Message = Messages.BadToken}, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult {Result = StatusCode.Unauthorized, Message = Messages.BadToken}, null);

            var tripDto = new TripDto
            {
                Id = _tripId,
                FromCityId = FromCity.Id,
                ToCountryId = ToCountry.Id <= 0 ? ToCity.Country?.Id ?? ToCountry.Id : ToCountry.Id,
                ToCityId = ToCity.Id < 1 ? default(int?) : ToCity.Id,
                FromCountryId = null,
                DateFrom = DateFrom,
                DateTo = DateTo,
                PreferredGender = Domain.StaticMembers.Genders.GenderList.First(x=>x.Value == PreferredGender).Key,
                Comment = Comment
            };
            var serializedFilter = JsonConvert.SerializeObject(tripDto);
            var result = await _network.LoadDataPostAsync(Url.UpdateTrip, serializedFilter, bearerToken);
            return result;
        }

        public async Task<(BaseResult baseResult, string content)> RemoveTripAsync(int tripId)
        {
            if (tripId <= 0) throw new ArgumentOutOfRangeException(nameof(tripId));

            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult { Result = StatusCode.Unauthorized, Message = Messages.BadToken }, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult { Result = StatusCode.Unauthorized, Message = Messages.BadToken }, null);

            var fullUrl = $"{Url.RemoveTrip}{tripId}";

            var result = await _network.LoadDataGetAsync(fullUrl, bearerToken);

            return result;
        } 
    }
}