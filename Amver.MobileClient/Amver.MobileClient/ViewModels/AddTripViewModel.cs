using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.Domain.Dto;
using Amver.Domain.Models;
using Amver.Libraries.Network.Interfaces;
using Autofac;
using Newtonsoft.Json;
using Xamarin.Forms;
using Amver.MobileClient.Localization;

namespace Amver.MobileClient.ViewModels
{
    public class AddTripViewModel : INotifyPropertyChanged
    {
        private readonly INetwork _network;
        
        public event PropertyChangedEventHandler PropertyChanged;
        public CityDto FromCity { get; set; } = new CityDto();

        public CityDto ToCity { get; set; } = new CityDto();

        public CountryDto ToCountry { get; set; } = new CountryDto();

        public DateTime DateFrom { get; set; } = DateTime.UtcNow.AddDays(1);

        public DateTime CurrentDate { get; set; } = DateTime.UtcNow;

        public DateTime MaximumDate { get; set; } = DateTime.UtcNow.AddYears(1);

        public DateTime DateTo { get; set; } = DateTime.UtcNow.AddDays(1);
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public List<string> PreferredGenders { get; set; }
        
        public string PreferredGender { get; set; }

        public string Comment { get; set; }

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
        public AddTripViewModel()  
        {
            _network = App.Container.Resolve<INetwork>();

            PreferredGenders = Domain.StaticMembers.Genders.GenderList.Values.Take(3).ToList();
            PreferredGender = PreferredGenders.LastOrDefault();
        }
        public void OnPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public static List<string> GetNamesPreferredGendersEnum()
        {
            var list = Domain.StaticMembers.Genders.GenderList.Values.Take(3).ToList();
            return list;
        }

        public async Task<(BaseResult baseResult, string content)> AddTripAsync()
        {
            if (FromCity.Id <= 0 )
                return (new BaseResult
                {
                    Result = StatusCode.Error,
                    Message = AppResources.FromCityIsRequired
                }, null);
            
            if (ToCity.Id <= 0 && ToCountry.Id <=0)
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
                return (new BaseResult {Result = StatusCode.Unauthorized}, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult {Result = StatusCode.Unauthorized}, null);
            
            var tripDto = new TripDto
            {
                FromCityId = FromCity.Id,
                ToCountryId = ToCountry.Id <= 0 ? ToCity.Country == null ? ToCountry.Id : ToCity.Country.Id : ToCountry.Id,
                ToCityId = ToCity.Id,
                FromCountryId = null,
                DateFrom = DateFrom,
                DateTo = DateTo,
                PreferredGender = Domain.StaticMembers.Genders.GenderList.First(x=>x.Value == PreferredGender).Key,
                Comment = Comment,
                CreatedDate = CreatedDate
            };
            var serializedFilter = JsonConvert.SerializeObject(tripDto);
            var result = await _network.LoadDataPostAsync(Url.AddTrip, serializedFilter, bearerToken);

            return result;
        }
    }
}