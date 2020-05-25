using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.Domain.Dto;
using Amver.Domain.Models;
using Amver.Libraries.Network.Interfaces;
using Amver.MobileClient.Localization;
using Autofac;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace Amver.MobileClient.ViewModels
{
    public class UpdateProfileViewModel : INotifyPropertyChanged
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
        
        private DateTime _birthDay;
        public DateTime BirthDay
        {
            get => _birthDay;
            set
            {
                _birthDay = value;
                OnPropertyChanged(nameof(BirthDay));
            }
        }
        
        private string _gender;
        public string Gender
        {
            get => _gender;
            set
            {
                _gender = value;
                OnPropertyChanged(nameof(Gender));
            }
        }
        
        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }
        
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
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

        public DateTime CurrentDate { get; set; } = DateTime.UtcNow;

        public DateTime MaximumDate { get; set; } = DateTime.UtcNow.AddYears(-18);
        
        public DateTime MinimumDate { get; set; } = DateTime.UtcNow.AddYears(-100);

        public bool IsModalAuthWasOpen { get; set; }
        
        public List<string> Genders { get; set; }

        public UpdateProfileViewModel()  
        {
            _network = App.Container.Resolve<INetwork>();

            Genders = Domain.StaticMembers.Genders.GenderList.Values.Take(2).ToList();
            Gender = Genders.LastOrDefault();
        }
        
        public async Task<(BaseResult baseResult, string content)> LoadProfileAsync()
        {
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult { Result = StatusCode.Unauthorized, Message = Messages.BadToken }, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult { Result = StatusCode.Unauthorized, Message = Messages.BadToken }, null);

            var fullUrl = $"{Url.GetMyProfileForUpdate}";

            var result = await _network.LoadDataGetAsync(fullUrl, bearerToken);

            return result;
        } 
        
        public void FillProfile(string content)
        {
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException("Value cannot be null or empty.", nameof(content));
            
            var user = JsonConvert.DeserializeObject<UserDto>(content);
            var fromCity = new CityDto();
            if (user.CityId != null)
            {
                fromCity.Id = user.CityId.Value;
                fromCity.Name = $"{user.City}, {user.Country}";
            }

            FromCity =  user.CityId != null ? fromCity : new CityDto{Name = AppResources.EnterYourCity};
            Name = string.IsNullOrEmpty(user.FirstName) ? AppResources.Name : user.FirstName;
            Gender = user.GenderId != 0 ? Domain.StaticMembers.Genders.GenderList.First(x=>x.Key == user.GenderId).Value : string.Empty;
            BirthDay = user.BirthDay ?? CurrentDate;
            Email = string.IsNullOrEmpty(user.Email) ? AppResources.Email : user.Email;
            Comment = user.Comment ?? string.Empty;
        }
        public async Task<(BaseResult baseResult, string content)> UpdateProfile()
        {
            var validEmail = GetValidEmail(Email);
            if (Email == string.Empty || validEmail == null)
                return (new BaseResult
                {
                    Result = StatusCode.Error,
                    Message = AppResources.EmailIsNotValid
                }, null);

            if (Gender == default)
            {
                return (new BaseResult
                {
                    Result = StatusCode.Error,
                    Message = AppResources.SpecifyBirthDayGender
                }, null);
            }

            if (BirthDay > DateTime.UtcNow || string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email))
                return (new BaseResult
                {
                    Result = StatusCode.Error,
                    Message = AppResources.FieldIsAnEmpty
                }, null);
            if (BirthDay > DateTime.UtcNow.AddYears(-18))
                return (new BaseResult
                {
                    Result = StatusCode.Error,
                    Message = AppResources.AgeConstraint
                }, null);
            
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult {Result = StatusCode.Unauthorized, Message = Messages.BadToken}, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult {Result = StatusCode.Unauthorized, Message = Messages.BadToken}, null);

            var userDto = new UserDto
            {
                CityId = FromCity.Id < 1 ? 0 : FromCity.Id,
                BirthDay = BirthDay,
                Email = Email,
                GenderId = Domain.StaticMembers.Genders.GenderList.First(x=>x.Value == Gender).Key,
                Comment = Comment,
                FirstName = Name
            };
            var serializedFilter = JsonConvert.SerializeObject(userDto);
            var result = await _network.LoadDataPostAsync(Url.UpdateProfile, serializedFilter, bearerToken);
            return result;
        }
        
        public void OnPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private static MailAddress GetValidEmail(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return mailAddress;
            }
            catch
            {
                return null;
            }
        }
    }
}