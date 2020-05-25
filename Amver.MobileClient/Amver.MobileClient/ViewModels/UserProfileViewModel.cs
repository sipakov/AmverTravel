using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.Domain.Dto;
using Amver.Domain.Entities;
using Amver.Domain.Models;
using Amver.Libraries.Network.Interfaces;
using Amver.MobileClient.Models;
using Autofac;
using Newtonsoft.Json;
using Xamarin.Forms;
using Genders = Amver.Domain.StaticMembers.Genders;

namespace Amver.MobileClient.ViewModels
{
    public class UserProfileViewModel : INotifyPropertyChanged
    {
        private readonly INetwork _network;
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        private ProfileForView _profile;
        
        public ProfileForView Profile
        {
            get => _profile;
            private set
            {
                _profile = value;
                OnPropertyChanged(nameof(Profile));
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

        private bool _isVisiblePhoto;
        public bool IsVisiblePhoto
        {
            get => _isVisiblePhoto;
            set
            {
                _isVisiblePhoto = value;
                OnPropertyChanged(nameof(IsVisiblePhoto));
            }
        }

        public UserProfileViewModel()  
        {
            _network = App.Container.Resolve<INetwork>();
        }
        
        public void OnPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public async Task<(BaseResult baseResult, string content)> LoadProfileAsync(int userId)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess || string.IsNullOrEmpty(bearerTokenObj.ToString()))
            {
                var fullUrl = $"{Url.GetUserProfile}{userId}";

                var result = await _network.LoadDataGetAsync(fullUrl, null);

                return result;    
            }
            else
            {
                if (!(bearerTokenObj is string bearerToken))
                    return (new BaseResult { Result = StatusCode.Unauthorized, Message = Messages.BadToken }, null);
            
                var fullUrl = $"{Url.GetAuthUserProfile}{userId}";

                var result = await _network.LoadDataGetAsync(fullUrl, bearerToken);

                return result;   
            }

        
        } 
        
        public void FillProfile(string content)
        {
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException("Value cannot be null or empty.", nameof(content));
            
            var user = JsonConvert.DeserializeObject<UserDto>(content);
            
            var profileForView = new ProfileForView
            {
                FirstName = user.FirstName,
                Age = user.BirthDay != null ? $"{CalculateAge(user.BirthDay.Value)}" : string.Empty,
                Gender = user.GenderId != 0 ? Genders.GenderList.First(x=>x.Key == user.GenderId).Value : string.Empty,
                BirthDay = user.BirthDay != null ? $"{user.BirthDay.Value:D}" : "-",
                From = user.City == null ? "-" : $"{user.City}",
                Comment = user.Comment ?? string.Empty,
                CreatedDate = user.CreatedDate.ToString("D"),
                UserImageUri = user.UserImageUri != null ? new UriImageSource
                {
                    CachingEnabled = true,
                    CacheValidity = new TimeSpan(30, 0, 0, 0),
                    Uri = new Uri(user.UserImageUri)
                } : new UriImageSource()
            };
            if (user.UserImageUri != null)
            {
                IsVisiblePhoto = true;
            }
            Profile = profileForView;
        }
        public async Task<(BaseResult baseResult, string content)> BlockUserAsync(int targetUserIdToBlock)
        {
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult { Result = StatusCode.Unauthorized, Message = Messages.BadToken }, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult { Result = StatusCode.Unauthorized, Message = Messages.BadToken }, null);

            var fullUrl = $"{Url.BlockUser}{targetUserIdToBlock}";

            var result = await _network.LoadDataGetAsync(fullUrl, bearerToken);

            return result;
        }
        
        public async Task<(BaseResult baseResult, string content)> UnblockUserAsync(int targetUserIdToBlock)
        {
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult { Result = StatusCode.Unauthorized, Message = Messages.BadToken }, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult { Result = StatusCode.Unauthorized, Message = Messages.BadToken }, null);

            var fullUrl = $"{Url.UnblockUser}{targetUserIdToBlock}";

            var result = await _network.LoadDataGetAsync(fullUrl, bearerToken);

            return result;
        }
        
        public async Task<bool> CheckBlockUserAsync(int targetUserId)
        {
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess || string.IsNullOrEmpty(bearerTokenObj.ToString()))
                return false;

            if (!(bearerTokenObj is string bearerToken))
                return false;

            var fullUrl = $"{Url.CheckMyBlocked}{targetUserId}";

            var result = await _network.LoadDataGetAsync(fullUrl, bearerToken);
            switch (result.baseResult.Result)
            {
                case StatusCode.Ok:
                    var blocker = JsonConvert.DeserializeObject<UserToBlockedUser>(result.response);
                    if (blocker != null)
                    {
                        return true;
                    }
                    break;
                case StatusCode.Unauthorized:
                    return false;
                default:
                    break;
            }

            return false;
        }
        
        private static int CalculateAge(DateTime dateOfBirth)  
        {
            var age = DateTime.UtcNow.Year - dateOfBirth.Year;  
            if (DateTime.UtcNow.DayOfYear < dateOfBirth.DayOfYear)  
                age -= 1;  
  
            return age;  
        }
    }
}