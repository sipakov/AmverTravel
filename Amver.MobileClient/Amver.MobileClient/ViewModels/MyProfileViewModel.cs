using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Amver.Domain.Constants;
using Amver.Domain.Dto;
using Amver.Domain.Models;
using Amver.Libraries.Network.Interfaces;
using Amver.MobileClient.Models;
using Autofac;
using Newtonsoft.Json;
using Xamarin.Forms;
using Genders = Amver.Domain.StaticMembers.Genders;

namespace Amver.MobileClient.ViewModels
{
    public class MyProfileViewModel : INotifyPropertyChanged
    {
      private readonly INetwork _network;
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        private ProfileForView _profile;

        public byte[] MediaFileStream { get; set; }
        
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
        private string _prepareImage;
        public string PrepareImage
        {
            get => _prepareImage;
            set
            {
                _prepareImage = value;
                OnPropertyChanged(nameof(PrepareImage));
            }
        }
        private string _imageStatus;
        [DefaultValue(ImageStatusDefault)]
        public string ImageStatus
        {
            get => _imageStatus;
            set
            {
                _imageStatus = value;
                OnPropertyChanged(nameof(ImageStatus));
            }
        }
        private const string ImageStatusDefault = "Select Profile Image";

        public int UserId { get; set; }

        public bool IsModalAuthWasOpen { get; set; }

        public MyProfileViewModel()  
        {
            _network = App.Container.Resolve<INetwork>();
        }
        
        public void OnPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public async Task<(BaseResult baseResult, string content)> LoadProfileAsync()
        {
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult { Result = StatusCode.Unauthorized, Message = Messages.BadToken }, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult { Result = StatusCode.Unauthorized, Message = Messages.BadToken }, null);

            var fullUrl = $"{Url.GetMyProfile}";

            var result = await _network.LoadDataGetAsync(fullUrl, bearerToken);

            return result;
        } 
        
        public void FillProfile(string content)
        {
            if (string.IsNullOrEmpty(content))
                throw new ArgumentNullException("Value cannot be null or empty.", nameof(content));
            
            var user = JsonConvert.DeserializeObject<UserDto>(content);
            
            var profileForView = new ProfileForView
            {
                FirstName = user.FirstName,
                Age = user.BirthDay != null ? $"{CalculateAge(user.BirthDay.Value)}" : string.Empty,
                Gender = user.GenderId != 0 ? Genders.GenderList.First(x=>x.Key == user.GenderId).Value : string.Empty,
                BirthDay = user.BirthDay != null ? $"{user.BirthDay.Value:D}" : "-",
                Email = string.IsNullOrEmpty(user.Email) ? "-" : user.Email,
                From = user.City == null ? "-" : user.City,
                Comment = user.Comment ?? string.Empty,
                UserImageUri = user.UserImageUri != null ? new UriImageSource
                {
                    CachingEnabled = true,
                    CacheValidity = new TimeSpan(30, 0, 0, 0),
                    Uri = new Uri(user.UserImageUri)
                } : new UriImageSource()
            };
            UserId = user.Id;
            Profile = profileForView;
        }
        
        private static int CalculateAge(DateTime dateOfBirth)  
        {
            var age = DateTime.UtcNow.Year - dateOfBirth.Year;  
            if (DateTime.UtcNow.DayOfYear < dateOfBirth.DayOfYear)  
                age -= 1;  
  
            return age;  
        }

        public async Task<(BaseResult baseResult, string content)> UploadFileAsync()
        {
            var fullUrl = Url.UploadUserPhoto;
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult { Result = StatusCode.Unauthorized, Message = Messages.BadToken }, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult { Result = StatusCode.Unauthorized, Message = Messages.BadToken }, null);

            var result = await _network.LoadFilePostAsync(fullUrl, MediaFileStream, bearerToken);

            return result;
        }
        
        public async Task<(BaseResult baseResult, string content)> RemoveFileAsync()
        {
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult {Result = StatusCode.Unauthorized, Message = Messages.BadToken}, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult {Result = StatusCode.Unauthorized, Message = Messages.BadToken}, null);
            
            var fullUrl = Url.RemoveProfileImage;

            var result = await _network.LoadDataGetAsync(fullUrl, bearerToken);

            return result;
        }
        
        public async Task<(BaseResult baseResult, string content)> RemoveProfileAsync()
        {
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return (new BaseResult {Result = StatusCode.Unauthorized, Message = Messages.BadToken}, null);

            if (!(bearerTokenObj is string bearerToken))
                return (new BaseResult {Result = StatusCode.Unauthorized, Message = Messages.BadToken}, null);
            
            var fullUrl = Url.RemoveProfile;

            var result = await _network.LoadDataGetAsync(fullUrl,  bearerToken);

            return result;
        }
        
        public async Task SignOut()
        {
            var isSuccess = Application.Current.Properties.TryGetValue(AuthOptions.AccessTokenAmver, out var bearerTokenObj);
            if (!isSuccess)
                return;

            var isSuccessFcmToken = Application.Current.Properties.TryGetValue(AuthOptions.FcmTokenAmver, out var fcmTokenAmver);

            var fcmTokenModel = new FcmTokenModel { FcmToken = isSuccessFcmToken ? fcmTokenAmver.ToString() : null };
            var serializedModel = JsonConvert.SerializeObject(fcmTokenModel);

            if (bearerTokenObj is string bearerToken)
            {
                await _network.LoadDataPostAsync(Url.SignOut, serializedModel, bearerToken).ConfigureAwait(false);
            }

            Application.Current.Properties[AuthOptions.AccessTokenAmver] = string.Empty;
        }
    }
}